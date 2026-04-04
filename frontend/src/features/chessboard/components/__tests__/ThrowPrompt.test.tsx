import { act, fireEvent, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import {
    idxToLogicalPoint,
    logicalPoint,
    pointToStr,
} from "@/features/point/pointUtils";
import ThrowPrompt, {
    INITIAL_CHARGE_DELAY_MS,
    CHARGE_OSCILLATION_STEP_DELAY_MS,
    CHARGE_STEP_MAX_DELAY_MS,
    CHARGE_STEP_MIN_DELAY_MS,
    ThrowSide,
    CHARGE_OSCILLATION_LOWER_INDEX,
    CHARGE_OSCILLATION_UPPER_INDEX,
} from "../ThrowPrompt";

import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import { BoardEffectType } from "../boardEffects/BoardEffects";
import { ThrowAimEffect } from "../boardEffects/ThrowAimLine";
import { LogicalPoint } from "@/features/point/types";
import { Move, Piece } from "../../lib/types";
import { GameColor } from "@/lib/apiClient";

interface ThrowTestMovesData {
    [ThrowSide.LEFT]: Move[];
    [ThrowSide.CENTER]: Move[];
    [ThrowSide.RIGHT]: Move[];
    all: Move[];
}

interface ThrowTestPointsData {
    [ThrowSide.LEFT]: LogicalPoint[];
    [ThrowSide.CENTER]: LogicalPoint[];
    [ThrowSide.RIGHT]: LogicalPoint[];
    all: LogicalPoint[];
}

interface ThrowTestData {
    throwerOrigin: LogicalPoint;
    piece: Piece;
    moves: ThrowTestMovesData;
    points: ThrowTestPointsData;
}

describe("ThrowPrompt", () => {
    let store: StoreApi<ChessboardStore>;

    const whitePiece = createFakePiece({
        position: logicalPoint({ x: 5, y: 1 }),
        color: GameColor.WHITE,
    });
    const whiteLeftThrow = createThrowTestData({
        piece: whitePiece,
        throwerOrigin: logicalPoint({ x: 6, y: 0 }),
        leftPoints: [
            logicalPoint({ x: 3, y: 2 }),
            logicalPoint({ x: 2, y: 3 }),
            logicalPoint({ x: 1, y: 4 }),
            logicalPoint({ x: 0, y: 5 }),
        ],
        centerPoints: [
            logicalPoint({ x: 4, y: 2 }),
            logicalPoint({ x: 3, y: 3 }),
            logicalPoint({ x: 2, y: 4 }),
            logicalPoint({ x: 1, y: 5 }),
            logicalPoint({ x: 0, y: 6 }),
        ],
        rightPoints: [
            logicalPoint({ x: 4, y: 3 }),
            logicalPoint({ x: 3, y: 4 }),
            logicalPoint({ x: 2, y: 5 }),
            logicalPoint({ x: 1, y: 6 }),
            logicalPoint({ x: 0, y: 7 }),
        ],
    });

    const whiteCenterThrow = createThrowTestData({
        piece: whitePiece,
        throwerOrigin: logicalPoint({ x: 5, y: 0 }),
        leftPoints: [
            logicalPoint({ x: 4, y: 2 }),
            logicalPoint({ x: 4, y: 3 }),
            logicalPoint({ x: 4, y: 4 }),
            logicalPoint({ x: 4, y: 5 }),
            logicalPoint({ x: 4, y: 6 }),
            logicalPoint({ x: 4, y: 7 }),
            logicalPoint({ x: 4, y: 8 }),
            logicalPoint({ x: 4, y: 9 }),
        ],
        centerPoints: [
            logicalPoint({ x: 5, y: 2 }),
            logicalPoint({ x: 5, y: 3 }),
            logicalPoint({ x: 5, y: 4 }),
            logicalPoint({ x: 5, y: 5 }),
            logicalPoint({ x: 5, y: 6 }),
            logicalPoint({ x: 5, y: 7 }),
            logicalPoint({ x: 5, y: 8 }),
            logicalPoint({ x: 5, y: 9 }),
        ],
        rightPoints: [
            logicalPoint({ x: 6, y: 2 }),
            logicalPoint({ x: 6, y: 3 }),
            logicalPoint({ x: 6, y: 4 }),
            logicalPoint({ x: 6, y: 5 }),
            logicalPoint({ x: 6, y: 6 }),
            logicalPoint({ x: 6, y: 7 }),
            logicalPoint({ x: 6, y: 8 }),
            logicalPoint({ x: 6, y: 9 }),
        ],
    });

    const whiteRightThrow = createThrowTestData({
        piece: whitePiece,
        throwerOrigin: logicalPoint({ x: 4, y: 0 }),
        leftPoints: [
            logicalPoint({ x: 6, y: 3 }),
            logicalPoint({ x: 7, y: 4 }),
            logicalPoint({ x: 8, y: 5 }),
            logicalPoint({ x: 9, y: 6 }),
        ],
        centerPoints: [
            logicalPoint({ x: 6, y: 2 }),
            logicalPoint({ x: 7, y: 3 }),
            logicalPoint({ x: 8, y: 4 }),
            logicalPoint({ x: 9, y: 5 }),
        ],
        rightPoints: [
            logicalPoint({ x: 7, y: 2 }),
            logicalPoint({ x: 8, y: 3 }),
            logicalPoint({ x: 9, y: 4 }),
        ],
    });

    const blackPiece = createFakePiece({
        position: logicalPoint({ x: 5, y: 8 }),
        color: GameColor.BLACK,
    });
    const blackLeftThrows = createThrowTestData({
        piece: blackPiece,
        throwerOrigin: logicalPoint({ x: 6, y: 9 }),
        leftPoints: [
            logicalPoint({ x: 3, y: 7 }),
            logicalPoint({ x: 2, y: 6 }),
            logicalPoint({ x: 1, y: 5 }),
            logicalPoint({ x: 0, y: 4 }),
        ],
        centerPoints: [
            logicalPoint({ x: 4, y: 7 }),
            logicalPoint({ x: 3, y: 6 }),
            logicalPoint({ x: 2, y: 5 }),
            logicalPoint({ x: 1, y: 4 }),
            logicalPoint({ x: 0, y: 3 }),
        ],
        rightPoints: [
            logicalPoint({ x: 4, y: 6 }),
            logicalPoint({ x: 3, y: 5 }),
            logicalPoint({ x: 2, y: 4 }),
            logicalPoint({ x: 1, y: 3 }),
            logicalPoint({ x: 0, y: 2 }),
        ],
    });

    const blackCenterThrows = createThrowTestData({
        piece: blackPiece,
        throwerOrigin: logicalPoint({ x: 5, y: 9 }),
        leftPoints: [
            logicalPoint({ x: 4, y: 7 }),
            logicalPoint({ x: 4, y: 6 }),
            logicalPoint({ x: 4, y: 5 }),
            logicalPoint({ x: 4, y: 4 }),
            logicalPoint({ x: 4, y: 3 }),
            logicalPoint({ x: 4, y: 2 }),
            logicalPoint({ x: 4, y: 1 }),
            logicalPoint({ x: 4, y: 0 }),
        ],
        centerPoints: [
            logicalPoint({ x: 5, y: 7 }),
            logicalPoint({ x: 5, y: 6 }),
            logicalPoint({ x: 5, y: 5 }),
            logicalPoint({ x: 5, y: 4 }),
            logicalPoint({ x: 5, y: 3 }),
            logicalPoint({ x: 5, y: 2 }),
            logicalPoint({ x: 5, y: 1 }),
            logicalPoint({ x: 5, y: 0 }),
        ],
        rightPoints: [
            logicalPoint({ x: 6, y: 7 }),
            logicalPoint({ x: 6, y: 6 }),
            logicalPoint({ x: 6, y: 5 }),
            logicalPoint({ x: 6, y: 4 }),
            logicalPoint({ x: 6, y: 3 }),
            logicalPoint({ x: 6, y: 2 }),
            logicalPoint({ x: 6, y: 1 }),
            logicalPoint({ x: 6, y: 0 }),
        ],
    });

    const blackRightThrows = createThrowTestData({
        piece: blackPiece,
        throwerOrigin: logicalPoint({ x: 4, y: 9 }),
        leftPoints: [
            logicalPoint({ x: 6, y: 6 }),
            logicalPoint({ x: 7, y: 5 }),
            logicalPoint({ x: 8, y: 4 }),
            logicalPoint({ x: 9, y: 3 }),
        ],
        centerPoints: [
            logicalPoint({ x: 6, y: 7 }),
            logicalPoint({ x: 7, y: 6 }),
            logicalPoint({ x: 8, y: 5 }),
            logicalPoint({ x: 9, y: 4 }),
        ],
        rightPoints: [
            logicalPoint({ x: 7, y: 7 }),
            logicalPoint({ x: 8, y: 6 }),
            logicalPoint({ x: 9, y: 5 }),
        ],
    });

    beforeEach(() => {
        store = createChessboardStore();
    });

    function createThrowTestData({
        piece,
        throwerOrigin,
        leftPoints,
        centerPoints,
        rightPoints,
    }: {
        piece: Piece;
        throwerOrigin: LogicalPoint;
        leftPoints: LogicalPoint[];
        centerPoints: LogicalPoint[];
        rightPoints: LogicalPoint[];
    }): ThrowTestData {
        const leftMoves = createTestMoves(throwerOrigin, leftPoints);
        const centerMoves = createTestMoves(throwerOrigin, centerPoints);
        const rightMoves = createTestMoves(throwerOrigin, rightPoints);

        const allPoints = [...leftPoints, ...centerPoints, ...rightPoints];
        const allMoves = [...leftMoves, ...centerMoves, ...rightMoves];

        return {
            throwerOrigin,
            piece,
            moves: {
                [ThrowSide.LEFT]: leftMoves,
                [ThrowSide.CENTER]: centerMoves,
                [ThrowSide.RIGHT]: rightMoves,
                all: allMoves,
            },
            points: {
                [ThrowSide.LEFT]: leftPoints,
                [ThrowSide.CENTER]: centerPoints,
                [ThrowSide.RIGHT]: rightPoints,
                all: allPoints,
            },
        };
    }

    function createTestMoves(
        throwerOrigin: LogicalPoint,
        points: LogicalPoint[],
    ): Move[] {
        return points.map((to) =>
            createFakeMove({
                from: whitePiece.position,
                to: logicalPoint(to),
                triggers: [throwerOrigin],
            }),
        );
    }

    function promptThrow(data: ThrowTestData): Promise<Move | null> {
        return store
            .getState()
            .promptThrow(data.throwerOrigin, data.piece, data.moves.all);
    }

    async function waitForNextChargeTick(idx: number) {
        const height = 10;
        const time = idx / (height - 1);
        const delay =
            CHARGE_STEP_MIN_DELAY_MS +
            (CHARGE_STEP_MAX_DELAY_MS - CHARGE_STEP_MIN_DELAY_MS) *
                (1 - time) ** 2;
        await act(() => vi.advanceTimersByTime(delay));
    }

    function assertAimEffect(data: ThrowTestData, side: ThrowSide): void {
        const boardEffects = store.getState().activeBoardEffects;
        expect(boardEffects.size).toBe(1);

        const effect = [...boardEffects.values()][0];
        expect(effect).toEqual<ThrowAimEffect>({
            type: BoardEffectType.THROW_AIM_LINE,
            from: data.piece.position,
            mid: data.points[side][0],
            to: data.points[side].at(-1)!,
        });
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
        whiteLeftThrow,
        whiteCenterThrow,
        whiteRightThrow,
        blackLeftThrows,
        blackCenterThrows,
        blackRightThrows,
    ])("should render overlay and throw line when prompted", async (data) => {
        promptThrow(data);

        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        assertAimEffect(data, ThrowSide.CENTER);

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
        assertSquaresEqual(selectedLineSquares, data.points[ThrowSide.CENTER]);

        expect(
            screen.queryByTestId("throwPromptSelectedSquare"),
        ).not.toBeInTheDocument();
    });

    it.each([
        whiteLeftThrow,
        whiteCenterThrow,
        whiteRightThrow,
        blackLeftThrows,
        blackCenterThrows,
        blackRightThrows,
    ])("should cycle through sides when clicking", async (data) => {
        promptThrow(data);

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        assertAimEffect(data, ThrowSide.CENTER);

        const overlay = screen.getByTestId("throwPromptOverlay");
        await user.click(overlay);
        assertAimEffect(data, ThrowSide.RIGHT);

        await user.click(overlay);
        assertAimEffect(data, ThrowSide.LEFT);

        await user.click(overlay);
        assertAimEffect(data, ThrowSide.CENTER);
    });

    it("should discard the prompt when right clicking", async () => {
        const promptPromise = promptThrow(whiteCenterThrow);

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
    });

    it.each([
        whiteLeftThrow,
        whiteCenterThrow,
        whiteRightThrow,
        blackLeftThrows,
        blackCenterThrows,
        blackRightThrows,
    ])("should animate throw power correctly", async (data) => {
        vi.useFakeTimers();

        const promptPromise = promptThrow(data);

        render(
            <ChessboardStoreContext.Provider value={store}>
                <ThrowPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const overlay = screen.getByTestId("throwPromptOverlay");

        // fireevent instead of userevent because userevent needs advancing timers
        fireEvent.pointerDown(overlay);

        await act(() => vi.advanceTimersByTime(INITIAL_CHARGE_DELAY_MS));
        const selectedSquare = screen.getByTestId("throwPromptSelectedSquare");
        const firstPoint = data.points[ThrowSide.CENTER][0];
        expect(selectedSquare.getAttribute("data-position")).toBe(
            pointToStr(firstPoint),
        );

        const pointsFromClosest = data.points[ThrowSide.CENTER];
        for (let i = 1; i < pointsFromClosest.length; i++) {
            await waitForNextChargeTick(i - 1);
            expect(selectedSquare.getAttribute("data-position")).toBe(
                pointToStr(pointsFromClosest[i]),
            );
        }

        const upperBound = Math.max(
            0,
            pointsFromClosest.length - 1 - CHARGE_OSCILLATION_UPPER_INDEX,
        );
        const lowerBound = Math.max(
            0,
            pointsFromClosest.length - 1 - CHARGE_OSCILLATION_LOWER_INDEX,
        );
        // get to the upper bound
        for (let i = pointsFromClosest.length - 1; i > upperBound; i--) {
            expect(selectedSquare.getAttribute("data-position")).toBe(
                pointToStr(pointsFromClosest[i]),
            );
            await act(() =>
                vi.advanceTimersByTime(CHARGE_OSCILLATION_STEP_DELAY_MS),
            );
        }

        let currentIdx = upperBound;
        let oscillating = true;
        let oscillationDirection = -1;
        while (oscillating) {
            await act(() =>
                vi.advanceTimersByTime(CHARGE_OSCILLATION_STEP_DELAY_MS),
            );

            currentIdx =
                oscillationDirection === 1
                    ? Math.min(pointsFromClosest.length - 1, currentIdx + 1)
                    : Math.max(0, currentIdx - 1);

            expect(selectedSquare.getAttribute("data-position")).toBe(
                pointToStr(pointsFromClosest[currentIdx]),
            );

            if (currentIdx <= lowerBound) {
                oscillationDirection = 1;
            } else if (currentIdx >= upperBound) {
                oscillationDirection = -1;
            }

            // stop after one full back-and-forth cycle
            if (currentIdx === upperBound && oscillationDirection === -1) {
                oscillating = false;
            }
        }

        fireEvent.pointerUp(overlay);
        const result = await act(() => promptPromise);
        expect(result?.to).toEqual(pointsFromClosest[upperBound]);
    });
});
