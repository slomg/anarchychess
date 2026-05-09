import { act, render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import {
    createFakeBoardPieces,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";
import { logicalPoint, screenPoint } from "@/features/point/pointUtils";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import SetupAnalysisPositionPage from "../SetupAnalysisPositionPage";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { GameColor, getNextLegalMoves } from "@/lib/apiClient";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";
import { AnalysisPageType } from "../AnalysisSide";

vi.mock("@/lib/apiClient/definition");

describe("SetupAnalysisPositionPage", () => {
    let store: StoreApi<ChessboardStore>;
    let expectedLegalMoves: LegalMoves;

    const setSelectedPageMock = vi.fn();
    const getNextLegalMovesMock = vi.mocked(getNextLegalMoves);

    const clearSetupModeBoardMock = vi.fn();
    const resetSetupModeBoardMock = vi.fn();
    const setSetupModeSideToMoveMock = vi.fn();

    beforeEach(() => {
        mockScrollTo();
        store = createChessboardStore();
        store.setState({
            boardRect: {
                left: 0,
                top: 0,
                width: 100,
                height: 100,
            } as DOMRect,
            clearSetupModeBoard: clearSetupModeBoardMock,
            resetSetupModeBoard: resetSetupModeBoardMock,
            setSetupModeSideToMove: setSetupModeSideToMoveMock,
        });

        const movePaths = [createFakeMovePath()];
        getNextLegalMovesMock.mockResolvedValue({
            error: undefined,
            data: movePaths,
            response: new Response(),
        });

        expectedLegalMoves = decodeMovePathIntoLegalMoves(movePaths);
    });

    it("should render move history toolbar with the correct buttons", () => {
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupAnalysisPositionPage
                    setSelectedPage={setSelectedPageMock}
                />
            </ChessboardStoreContext.Provider>,
        );

        const toolbar = screen.getByTestId("moveHistoryToolbar");
        expect(toolbar).toBeInTheDocument();

        expect(toolbar).toHaveClass("order-1 lg:order-2");
        expect(within(toolbar).getByTitle("Go Back")).toBeInTheDocument();
        expect(within(toolbar).getByTitle("Reset Board")).toBeInTheDocument();
        expect(within(toolbar).getByTitle("Clear Board")).toBeInTheDocument();
        expect(within(toolbar).getByTitle("Flip Board")).toBeInTheDocument();

        expect(setSelectedPageMock).not.toHaveBeenCalled();
    });

    it("should render setup position pieces", () => {
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupAnalysisPositionPage
                    setSelectedPage={setSelectedPageMock}
                />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByTestId("setupPositionPieces")).toBeInTheDocument();
    });

    it("should reset board when clicking reset board button", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupAnalysisPositionPage
                    setSelectedPage={setSelectedPageMock}
                />
            </ChessboardStoreContext.Provider>,
        );

        await user.click(screen.getByTitle("Reset Board"));

        expect(resetSetupModeBoardMock).toHaveBeenCalledOnce();
    });

    it("should clear board when clicking clear board button", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupAnalysisPositionPage
                    setSelectedPage={setSelectedPageMock}
                />
            </ChessboardStoreContext.Provider>,
        );

        await user.click(screen.getByTitle("Clear Board"));

        expect(clearSetupModeBoardMock).toHaveBeenCalledOnce();
    });

    it.each([GameColor.WHITE, GameColor.BLACK])(
        "should set selector default value to viewing position side to move",
        (sideToMove) => {
            const positionHistory = new PositionHistory({
                pieces: createFakeBoardPieces(),
            });
            positionHistory.addNextPosition(
                createFakePositionProps({ sideToMove }),
            );
            store.setState({ positionHistory });

            render(
                <ChessboardStoreContext.Provider value={store}>
                    <SetupAnalysisPositionPage
                        setSelectedPage={setSelectedPageMock}
                    />
                </ChessboardStoreContext.Provider>,
            );

            expect(
                screen.getByTestId("setupPositionSideToMove"),
            ).toHaveAttribute("data-selected", sideToMove.toString());
        },
    );

    it.each([GameColor.WHITE, GameColor.BLACK])(
        "should set selector default value to root side to move",
        (sideToMove) => {
            const positionHistory = new PositionHistory({
                pieces: createFakeBoardPieces(),
                sideToMove,
            });
            store.setState({ positionHistory });

            render(
                <ChessboardStoreContext.Provider value={store}>
                    <SetupAnalysisPositionPage
                        setSelectedPage={setSelectedPageMock}
                    />
                </ChessboardStoreContext.Provider>,
            );

            expect(
                screen.getByTestId("setupPositionSideToMove"),
            ).toHaveAttribute("data-selected", sideToMove.toString());
        },
    );

    it("should change setup side to move when clicking", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupAnalysisPositionPage
                    setSelectedPage={setSelectedPageMock}
                />
            </ChessboardStoreContext.Provider>,
        );

        await user.click(
            within(screen.getByTestId("setupPositionSideToMove")).getByTestId(
                `selector-${GameColor.BLACK}`,
            ),
        );

        expect(setSetupModeSideToMoveMock).toHaveBeenCalledExactlyOnceWith(
            GameColor.BLACK,
        );
    });

    it("should change page when clicking go back without changing the position", async () => {
        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupAnalysisPositionPage
                    setSelectedPage={setSelectedPageMock}
                />
            </ChessboardStoreContext.Provider>,
        );

        await user.click(screen.getByTitle("Go Back"));

        expect(setSelectedPageMock).toHaveBeenCalledExactlyOnceWith(
            AnalysisPageType.Main,
        );
        expect(getNextLegalMovesMock).not.toHaveBeenCalled();
    });

    it("should refetch legal moves if the position changed", async () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const pieces = BoardPieces.fromPieces(piece);
        store.setState({ pieces });

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <SetupAnalysisPositionPage
                    setSelectedPage={setSelectedPageMock}
                />
            </ChessboardStoreContext.Provider>,
        );

        const { selectPiece, makeSetupModeMove } = store.getState();
        act(() => {
            selectPiece(piece.id);
            makeSetupModeMove(screenPoint({ x: 20, y: 20 }));
        });

        const positionHistory = store.getState().positionHistory;

        await user.click(screen.getByTitle("Go Back"));
        expect(setSelectedPageMock).toHaveBeenCalledExactlyOnceWith(
            AnalysisPageType.Main,
        );
        expect(getNextLegalMovesMock).toHaveBeenCalledExactlyOnceWith({
            query: { fen: positionHistory.root.fen },
        });

        expect(store.getState().getViewedPositionLegalMoves()).toEqual(
            expectedLegalMoves,
        );
    });
});
