import { act, fireEvent, render, screen } from "@testing-library/react";
import userEvent, { UserEvent } from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    BLACK_CENTER_THROWS,
    BLACK_LEFT_THROWS,
    BLACK_RIGHT_THROWS,
    createThrowTestData,
    ThrowTestData,
    WHITE_CENTER_THROWS,
    WHITE_LEFT_THROWS,
    WHITE_RIGHT_THROWS,
} from "@/lib/testUtils/throwTestData";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import {
    idxToLogicalPoint,
    logicalPoint,
    offset,
    pointToStr,
} from "@/features/point/pointUtils";
import ThrowPrompt, {
    DEFAULT_THROW_STEP_SIZE,
    THROW_COMMIT_DELAY_MS,
    THROW_INTENT_DELAY_MS,
} from "../ThrowPrompt";

import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import { PersistentBoardEffectType } from "../../stores/boardEffectsSlice";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import { ThrowAimEffect } from "../boardEffects/ThrowAimLine";
import { LogicalPoint } from "@/features/point/types";
import { GameColor } from "@/lib/apiClient";
import { Move } from "../../lib/types";

describe("ThrowPrompt", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    async function swipeInDirection({
        data,
        steps,
        user,
        direction,
        down,
        up,
    }: {
        data: ThrowTestData;
        steps: number;
        user: UserEvent;
        direction: "forward" | "side";
        down?: boolean;
        up?: boolean;
    }): Promise<void> {
        vi.useFakeTimers({ shouldAdvanceTime: true });

        down ??= true;
        down ??= true;

        const overlay = screen.getByTestId("throwPromptOverlay");

        let dirX: number;
        let dirY: number;
        if (direction === "forward") {
            dirX = data.direction.x;
            dirY = -data.direction.y;
        } else {
            dirX = data.direction.y;
            dirY = data.direction.x;
        }

        const start = { x: 0, y: 0 };
        if (down) {
            await user.pointer([
                {
                    target: overlay,
                    coords: { x: start.x, y: start.y },
                    keys: "[MouseLeft>]",
                },
            ]);
            await act(() => vi.advanceTimersToNextFrame());
        }

        await user.pointer([
            {
                coords: {
                    x: start.x + dirX * DEFAULT_THROW_STEP_SIZE * steps,
                    y: start.y + dirY * DEFAULT_THROW_STEP_SIZE * steps,
                },
            },
        ]);
        await act(() => vi.advanceTimersToNextFrame());

        if (up) {
            await user.pointer([
                {
                    keys: "[/MouseLeft]",
                },
            ]);
            await act(() => vi.advanceTimersToNextFrame());
        }
    }

    function promptThrow(data: ThrowTestData): Promise<Move | null> {
        return store
            .getState()
            .promptThrow(data.throwerOrigin, data.piece, data.moves.all);
    }

    function assertAimEffect({
        data,
        sideIdx,
        pointIdx,
    }: {
        data: ThrowTestData;
        sideIdx: number;
        pointIdx: number;
    }): void {
        const boardEffects = store.getState().activePersistentBoardEffects;
        expect(boardEffects.size).toBe(1);

        const effect = [...boardEffects.values()][0];
        const to = data.points.sides[sideIdx][pointIdx];
        expect(effect).toEqual<ThrowAimEffect>({
            type: PersistentBoardEffectType.THROW_AIM_LINE,
            from: data.piece.position,
            mid: data.points.sides[sideIdx][0],
            to,
        });

        const selectedSquare = screen.queryByTestId(
            "throwPromptSelectedSquare",
        );
        expect(selectedSquare).toBeInTheDocument();
        expect(selectedSquare).toHaveAttribute("data-position", pointToStr(to));
    }

    function assertCleanedUp() {
        const boardEffects = store.getState().activePersistentBoardEffects;
        expect(boardEffects.size).toBe(0);
        expect(
            screen.queryByTestId("throwPromptOverlay"),
        ).not.toBeInTheDocument();
    }

    function assertMidAimEffect({
        data,
        sideIdx,
    }: {
        data: ThrowTestData;
        sideIdx: number;
    }): void {
        assertAimEffect({
            data,
            sideIdx,
            pointIdx: getMidAimIdx({ data, sideIdx }),
        });
    }

    function getMidAimIdx({
        data,
        sideIdx,
    }: {
        data: ThrowTestData;
        sideIdx: number;
    }) {
        return Math.floor((data.points.sides[sideIdx].length - 1) / 2);
    }

    function assertSquaresEqual(
        squares: HTMLElement[],
        expected: LogicalPoint[],
    ): void {
        const squarePoints = squares.map((x) =>
            x.getAttribute("data-position"),
        );
        expect(squarePoints).toEqual(expected.map(pointToStr));
    }

    it("should not render anything before prompted", () => {
        const { container } = render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        expect(container.firstChild).toBeNull();
    });

    it.each([
        WHITE_LEFT_THROWS,
        WHITE_CENTER_THROWS,
        WHITE_RIGHT_THROWS,
        BLACK_LEFT_THROWS,
        BLACK_CENTER_THROWS,
        BLACK_RIGHT_THROWS,
    ])("should render overlay when prompted", (data) => {
        promptThrow(data);

        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const overlaySquares = screen.getAllByTestId(
            "throwPromptOverlaySquare",
        );
        const pointsSet = new Set(data.points.all.map(pointToStr));
        const expectedOverlayPoints: LogicalPoint[] = Array.from(
            { length: 100 },
            (_, i) => idxToLogicalPoint(i, 10),
        ).filter((x) => !pointsSet.has(pointToStr(x)));
        assertSquaresEqual(overlaySquares, expectedOverlayPoints);

        const selectedLineSquares = screen.getAllByTestId(
            "throwPromptSelectedLineSquare",
        );
        assertSquaresEqual(selectedLineSquares, data.points.sides[0]);
    });

    it.each([
        WHITE_LEFT_THROWS,
        WHITE_CENTER_THROWS,
        WHITE_RIGHT_THROWS,
        BLACK_LEFT_THROWS,
        BLACK_CENTER_THROWS,
        BLACK_RIGHT_THROWS,
    ])("should draw a throw aim line", (data) => {
        promptThrow(data);

        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        assertMidAimEffect({
            data,
            sideIdx: 0,
        });
    });

    it.each([
        WHITE_LEFT_THROWS,
        WHITE_CENTER_THROWS,
        WHITE_RIGHT_THROWS,
        BLACK_LEFT_THROWS,
        BLACK_CENTER_THROWS,
        BLACK_RIGHT_THROWS,
    ])("should cycle through sides when clicking", async (data) => {
        promptThrow(data);

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const originalMid = getMidAimIdx({ data, sideIdx: 0 });
        assertAimEffect({ data, sideIdx: 0, pointIdx: originalMid });

        const overlay = screen.getByTestId("throwPromptOverlay");
        await user.click(overlay);
        assertAimEffect({ data, sideIdx: 1, pointIdx: originalMid });

        await user.click(overlay);
        assertAimEffect({ data, sideIdx: 2, pointIdx: originalMid });

        await user.click(overlay);
        assertAimEffect({ data, sideIdx: 0, pointIdx: originalMid });
    });

    it("should skip unavailable sides when clicking", async () => {
        const data = createThrowTestData({
            direction: offset({ x: 0, y: 1 }),
            piece: createFakePiece({
                position: logicalPoint({ x: 9, y: 1 }),
                color: GameColor.WHITE,
            }),
            throwerOrigin: logicalPoint({ x: 9, y: 0 }),
            leftPoints: [
                logicalPoint({ x: 8, y: 2 }),
                logicalPoint({ x: 8, y: 3 }),
                logicalPoint({ x: 8, y: 4 }),
                logicalPoint({ x: 8, y: 5 }),
            ],
            centerPoints: [
                logicalPoint({ x: 9, y: 2 }),
                logicalPoint({ x: 9, y: 3 }),
                logicalPoint({ x: 9, y: 4 }),
                logicalPoint({ x: 9, y: 5 }),
            ],
            rightPoints: [],
        });
        promptThrow(data);

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        assertMidAimEffect({ data, sideIdx: 0 });

        const overlay = screen.getByTestId("throwPromptOverlay");
        await user.click(overlay);
        assertMidAimEffect({ data, sideIdx: 1 });

        await user.click(overlay);
        assertMidAimEffect({ data, sideIdx: 0 });
    });

    it("should discard the prompt when right clicking", async () => {
        const promptPromise = promptThrow(WHITE_CENTER_THROWS);

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const overlay = screen.getByTestId("throwPromptOverlay");
        await user.pointer({ keys: "[MouseRight]", target: overlay });

        const result = await promptPromise;
        expect(result).toBeNull();

        assertCleanedUp();
    });

    it("should scroll selected point down to 0", () => {
        promptThrow(WHITE_CENTER_THROWS);

        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const mid = getMidAimIdx({ data: WHITE_CENTER_THROWS, sideIdx: 0 });

        assertAimEffect({
            data: WHITE_CENTER_THROWS,
            sideIdx: 0,
            pointIdx: mid,
        });

        const overlay = screen.getByTestId("throwPromptOverlay");

        let expected = mid;
        for (let i = 0; i < WHITE_CENTER_THROWS.points.sides[0].length; i++) {
            fireEvent.wheel(overlay, { deltaY: 100 });

            expected = Math.max(expected - 1, 0);
            assertAimEffect({
                data: WHITE_CENTER_THROWS,
                sideIdx: 0,
                pointIdx: expected,
            });
        }

        assertAimEffect({
            data: WHITE_CENTER_THROWS,
            sideIdx: 0,
            pointIdx: 0,
        });
    });

    it.each([
        {
            viewingFrom: GameColor.WHITE,
            pieceColor: GameColor.WHITE,
            expectedInvert: true,
        },
        {
            viewingFrom: GameColor.BLACK,
            pieceColor: GameColor.WHITE,
            expectedInvert: false,
        },
        {
            viewingFrom: GameColor.WHITE,
            pieceColor: GameColor.BLACK,
            expectedInvert: false,
        },
        {
            viewingFrom: GameColor.BLACK,
            pieceColor: GameColor.BLACK,
            expectedInvert: true,
        },
    ])(
        "should scroll correctly based on viewingFrom relative to piece color",
        ({ viewingFrom, pieceColor, expectedInvert }) => {
            const data =
                pieceColor === GameColor.WHITE
                    ? WHITE_CENTER_THROWS
                    : BLACK_CENTER_THROWS;

            store.setState({ viewingFrom });
            promptThrow(data);

            render(
                <ChessboardStoreContext.Provider value={store}>
                    <ThrowPrompt />
                </ChessboardStoreContext.Provider>,
            );

            const overlay = screen.getByTestId("throwPromptOverlay");
            fireEvent.wheel(overlay, { deltaY: 100 });

            const mid = getMidAimIdx({ data, sideIdx: 0 });
            const expected = expectedInvert ? mid - 1 : mid + 1;
            assertAimEffect({
                data,
                sideIdx: 0,
                pointIdx: expected,
            });
        },
    );

    it("should scroll selected point up to max", () => {
        promptThrow(WHITE_CENTER_THROWS);

        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const mid = getMidAimIdx({ data: WHITE_CENTER_THROWS, sideIdx: 0 });
        const max = WHITE_CENTER_THROWS.points.sides[0].length - 1;

        assertAimEffect({
            data: WHITE_CENTER_THROWS,
            sideIdx: 0,
            pointIdx: mid,
        });

        const overlay = screen.getByTestId("throwPromptOverlay");
        let expected = mid;
        for (let i = 0; i < WHITE_CENTER_THROWS.points.sides[0].length; i++) {
            fireEvent.wheel(overlay, { deltaY: -100 });

            expected = Math.min(expected + 1, max);

            assertAimEffect({
                data: WHITE_CENTER_THROWS,
                sideIdx: 0,
                pointIdx: expected,
            });
        }

        assertAimEffect({
            data: WHITE_CENTER_THROWS,
            sideIdx: 0,
            pointIdx: max,
        });
    });

    it.each([
        WHITE_LEFT_THROWS,
        WHITE_CENTER_THROWS,
        WHITE_RIGHT_THROWS,
        BLACK_LEFT_THROWS,
        BLACK_CENTER_THROWS,
        BLACK_RIGHT_THROWS,
    ])(
        "should move selected point when swiping vertically relative to direction",
        async (data) => {
            promptThrow(data);

            const user = userEvent.setup();
            render(
                <ChessboardStoreContext.Provider value={store}>
                    <ThrowPrompt />
                </ChessboardStoreContext.Provider>,
            );

            const mid = getMidAimIdx({ data, sideIdx: 0 });
            const max = data.points.sides[0].length - 1;
            const expectedUp = Math.min(mid + 2, max);
            const expectedDown = Math.max(expectedUp - 2, 0);

            await swipeInDirection({
                data,
                user,
                steps: 2,
                direction: "forward",
            });
            assertAimEffect({
                data,
                sideIdx: 0,
                pointIdx: expectedUp,
            });

            await swipeInDirection({
                data,
                user,
                steps: -2,
                direction: "forward",
            });
            assertAimEffect({
                data,
                sideIdx: 0,
                pointIdx: expectedDown,
            });
        },
    );

    it.each([
        WHITE_LEFT_THROWS,
        WHITE_CENTER_THROWS,
        WHITE_RIGHT_THROWS,
        BLACK_LEFT_THROWS,
        BLACK_CENTER_THROWS,
        BLACK_RIGHT_THROWS,
    ])(
        "should change side when swiping horizontally relative to direction",
        async (data) => {
            promptThrow(data);

            const user = userEvent.setup();
            render(
                <ChessboardStoreContext.Provider value={store}>
                    <ThrowPrompt />
                </ChessboardStoreContext.Provider>,
            );

            const prevMid = getMidAimIdx({ data, sideIdx: 0 });

            await swipeInDirection({
                data,
                user,
                steps: 1,
                direction: "side",
            });

            assertAimEffect({
                data,
                sideIdx: 1,
                pointIdx: prevMid,
            });

            await swipeInDirection({
                data,
                user,
                steps: 1,
                direction: "side",
            });

            assertAimEffect({
                data,
                sideIdx: 2,
                pointIdx: prevMid,
            });

            await swipeInDirection({
                data,
                user,
                steps: -2,
                direction: "side",
            });

            assertAimEffect({
                data,
                sideIdx: 0,
                pointIdx: prevMid,
            });
        },
    );

    it("should blink and commit after long press", async () => {
        const promptPromise = promptThrow(WHITE_CENTER_THROWS);
        vi.useFakeTimers();

        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        // we can't use userEvent because we're faking the timer
        const overlay = screen.getByTestId("throwPromptOverlay");
        fireEvent.pointerDown(overlay);

        const selectedSquare = screen.getByTestId("throwPromptSelectedSquare");
        expect(selectedSquare).not.toHaveClass("animate-fast-blink");

        await act(() => vi.advanceTimersByTime(THROW_INTENT_DELAY_MS));
        expect(selectedSquare).toHaveClass("animate-fast-blink");

        await act(async () => {
            vi.advanceTimersByTime(THROW_COMMIT_DELAY_MS);
            await flushMicrotasks();
        });

        const mid = getMidAimIdx({ data: WHITE_CENTER_THROWS, sideIdx: 0 });
        expect(await promptPromise).toEqual(
            WHITE_CENTER_THROWS.moves.sides[0][mid],
        );
        assertCleanedUp();
    });

    it("should cancel confirmation if moved", async () => {
        promptThrow(WHITE_CENTER_THROWS);
        vi.useFakeTimers({ shouldAdvanceTime: true });

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const overlay = screen.getByTestId("throwPromptOverlay");
        await user.pointer({
            target: overlay,
            keys: "[MouseLeft>]",
        });

        const selectedSquare = screen.getByTestId("throwPromptSelectedSquare");
        expect(selectedSquare).not.toHaveClass("animate-fast-blink");

        await act(() => vi.advanceTimersByTime(THROW_INTENT_DELAY_MS));
        expect(selectedSquare).toHaveClass("animate-fast-blink");

        await swipeInDirection({
            data: WHITE_CENTER_THROWS,
            steps: 1,
            user,
            direction: "forward",
        });

        expect(selectedSquare).not.toHaveClass("animate-fast-blink");
        expect(overlay).toBeInTheDocument();
    });

    it("should treat each point snap point as a new base", async () => {
        promptThrow(WHITE_CENTER_THROWS);

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const mid = getMidAimIdx({ data: WHITE_CENTER_THROWS, sideIdx: 0 });

        await swipeInDirection({
            data: WHITE_CENTER_THROWS,
            user,
            steps: 1,
            direction: "forward",
            up: false,
        });

        assertAimEffect({
            data: WHITE_CENTER_THROWS,
            sideIdx: 0,
            pointIdx: mid + 1,
        });

        await swipeInDirection({
            data: WHITE_CENTER_THROWS,
            user,
            steps: 0.5,
            direction: "forward",
            down: false,
        });

        assertAimEffect({
            data: WHITE_CENTER_THROWS,
            sideIdx: 0,
            pointIdx: mid + 1,
        });
    });

    it("should treat each side snap point as a new base", async () => {
        promptThrow(WHITE_CENTER_THROWS);

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const mid = getMidAimIdx({ data: WHITE_CENTER_THROWS, sideIdx: 0 });

        await swipeInDirection({
            data: WHITE_CENTER_THROWS,
            user,
            steps: 1,
            direction: "side",
            up: false,
        });

        assertAimEffect({
            data: WHITE_CENTER_THROWS,
            sideIdx: 1,
            pointIdx: mid,
        });

        await swipeInDirection({
            data: WHITE_CENTER_THROWS,
            user,
            steps: 0.5,
            direction: "side",
            down: false,
        });

        assertAimEffect({
            data: WHITE_CENTER_THROWS,
            sideIdx: 1,
            pointIdx: mid,
        });
    });
});
