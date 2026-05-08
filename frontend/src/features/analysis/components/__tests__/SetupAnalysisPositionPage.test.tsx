import { act, render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { decodeMovePathIntoLegalMoves } from "@/features/liveGame/lib/moveDecoder";
import { createFakeMovePath } from "@/lib/testUtils/fakers/movePathFaker";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import { logicalPoint, screenPoint } from "@/features/point/pointUtils";
import SetupAnalysisPositionPage from "../SetupAnalysisPositionPage";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";
import { getNextLegalMoves } from "@/lib/apiClient";
import { AnalysisPageType } from "../AnalysisSide";

vi.mock("@/lib/apiClient/definition");

describe("SetupAnalysisPositionPage", () => {
    let store: StoreApi<ChessboardStore>;
    let expectedLegalMoves: LegalMoves;

    const setSelectedPageMock = vi.fn();
    const getNextLegalMovesMock = vi.mocked(getNextLegalMoves);

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
        expect(within(toolbar).getByTitle("Flip Board")).toBeInTheDocument();

        expect(setSelectedPageMock).not.toHaveBeenCalled();
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
