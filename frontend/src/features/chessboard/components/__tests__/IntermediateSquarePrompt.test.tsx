import { fireEvent, render, screen } from "@testing-library/react";
import { logicalPoint } from "@/features/point/pointUtils";
import userEvent from "@testing-library/user-event";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import { StoreApi } from "zustand";
import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import IntermediateSquarePrompt from "../IntermediateSquarePrompt";
import { LogicalPoint } from "@/features/point/types";
import { PieceID } from "../../lib/types";

describe("IntermediateSquarePrompt", () => {
    let store: StoreApi<ChessboardStore>;

    const pieceId: PieceID = "test piece";

    beforeEach(() => {
        store = createChessboardStore();
    });

    const renderComponent = () =>
        render(
            <ChessboardStoreContext.Provider value={store}>
                <IntermediateSquarePrompt />
            </ChessboardStoreContext.Provider>,
        );

    it("should render nothing when nextIntermediates is empty", () => {
        store.setState({ pendingIntermediate: null });
        renderComponent();
        expect(
            screen.queryByTestId("intermediateSquarePromptOverlay"),
        ).toBeNull();
    });

    it("should render overlay and squares when nextIntermediates has points", () => {
        const points = [
            logicalPoint({ x: 1, y: 1 }),
            logicalPoint({ x: 2, y: 2 }),
        ];
        store.setState({
            pendingIntermediate: { nextOptions: points, pieceId },
        });

        renderComponent();

        expect(
            screen.getByTestId("intermediateSquarePromptOverlay"),
        ).toBeInTheDocument();

        const squares = screen.getAllByTestId("intermediateSquare");
        expect(squares).toHaveLength(points.length);
    });

    it("should call resolve intermediate with null when overlay is clicked", async () => {
        const user = userEvent.setup();
        const points = [logicalPoint({ x: 1, y: 1 })];

        let resolvedPoint: LogicalPoint | null = logicalPoint({ x: 6, y: 9 });
        store.setState({
            pendingIntermediate: { nextOptions: points, pieceId },
            resolveNextIntermediate: (point) => (resolvedPoint = point),
        });

        renderComponent();

        const overlay = screen.getByTestId("intermediateSquarePromptOverlay");
        await user.pointer({ target: overlay, keys: "[MouseLeft]" });

        expect(resolvedPoint).toBeNull();
    });

    it("should select the correct intermediate when a square is clicked", async () => {
        const user = userEvent.setup();
        const points = [
            logicalPoint({ x: 1, y: 1 }),
            logicalPoint({ x: 2, y: 2 }),
        ];
        let resolvedPoint: LogicalPoint | null = null;
        store.setState({
            pendingIntermediate: { nextOptions: points, pieceId },
            resolveNextIntermediate: (point) => (resolvedPoint = point),
        });

        renderComponent();

        const squares = screen.getAllByTestId("intermediateSquare");
        await user.click(squares[1]);

        expect(resolvedPoint).toEqual(points[1]);
    });

    it("should require pointerdown on the square before resolving", () => {
        const points = [logicalPoint({ x: 7, y: 7 })];

        let resolvedPoint: LogicalPoint | null = null;
        store.setState({
            pendingIntermediate: { nextOptions: points, pieceId },
            resolveNextIntermediate: (point) => (resolvedPoint = point),
        });

        renderComponent();

        const square = screen.getByTestId("intermediateSquare");

        fireEvent.pointerUp(square);
        expect(resolvedPoint).toBeNull();

        fireEvent.pointerDown(square);
        fireEvent.pointerUp(square);

        expect(resolvedPoint).toEqual(points[0]);
    });
});
