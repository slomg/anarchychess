import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import MoveRow from "../MoveRow";

describe("MoveRow", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        chessboardStore = createChessboardStore();
    });

    it("should render white move correctly with row number", () => {
        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const whitePos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e4" }),
        );

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveRow whitePosition={whitePos} />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByText("1.")).toBeInTheDocument();
        expect(screen.getByText("e4")).toBeInTheDocument();
    });

    it("should render black move correctly with row number", () => {
        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const whitePos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e4" }),
        );
        const blackPos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e5" }),
        );
        chessboardStore.setState({ positionHistory });

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveRow whitePosition={whitePos} blackPosition={blackPos} />
            </ChessboardStoreContext.Provider>,
        );

        const [whiteButton, blackButton] = screen.getAllByRole("button");
        expect(whiteButton).toHaveTextContent("e4");
        expect(blackButton).toHaveTextContent("e5");
    });

    it("should apply selected class when viewing white move", () => {
        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const whitePos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e4" }),
        );
        const blackPos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e5" }),
        );
        positionHistory.goToPosition(whitePos.positionId);
        chessboardStore.setState({ positionHistory });

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveRow whitePosition={whitePos} blackPosition={blackPos} />
            </ChessboardStoreContext.Provider>,
        );

        const [whiteButton, blackButton] = screen.getAllByRole("button");
        expect(whiteButton).toHaveClass("bg-blue-300/30");
        expect(blackButton).not.toHaveClass("bg-blue-300/30");
    });

    it("should apply selected class when viewing black move", () => {
        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const whitePos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e4" }),
        );
        const blackPos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e5" }),
        );
        chessboardStore.setState({ positionHistory });

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveRow whitePosition={whitePos} blackPosition={blackPos} />
            </ChessboardStoreContext.Provider>,
        );

        const [whiteButton, blackButton] = screen.getAllByRole("button");
        expect(whiteButton).not.toHaveClass("bg-blue-300/30");
        expect(blackButton).toHaveClass("bg-blue-300/30");
    });

    it("should go to the white position when white move is clicked", async () => {
        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const whitePos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e4" }),
        );
        chessboardStore.setState({ positionHistory });

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveRow whitePosition={whitePos} />
            </ChessboardStoreContext.Provider>,
        );

        const whiteButton = screen.getByText("e4");
        await user.click(whiteButton);

        expect(chessboardStore.getState().positionHistory.viewingPosition).toBe(
            whitePos,
        );
    });

    it("should go to the black position when black move is clicked", async () => {
        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const whitePos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e4" }),
        );
        const blackPos = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e5" }),
        );
        chessboardStore.setState({ positionHistory });

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveRow whitePosition={whitePos} blackPosition={blackPos} />
            </ChessboardStoreContext.Provider>,
        );

        const blackButton = screen.getByText("e5");
        await user.click(blackButton);

        expect(
            chessboardStore.getState().positionHistory.viewingPosition
                ?.positionId,
        ).toBe(blackPos.positionId);
    });

    it("should apply alternating row background color correctly", () => {
        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const whitePos1 = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e4" }),
        );
        const blackPos1 = positionHistory.addNextPosition(
            createFakePositionProps({ san: "e5" }),
        );

        const whitePos2 = positionHistory.addNextPosition(
            createFakePositionProps({ san: "Nf3" }),
        );
        const blackPos2 = positionHistory.addNextPosition(
            createFakePositionProps({ san: "Nc6" }),
        );
        chessboardStore.setState({ positionHistory });

        const { rerender } = render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveRow whitePosition={whitePos1} blackPosition={blackPos1} />
            </ChessboardStoreContext.Provider>,
        );
        const oddRow = screen.getByTestId("moveRow");
        expect(oddRow).not.toHaveClass("bg-white/10");

        rerender(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveRow whitePosition={whitePos2} blackPosition={blackPos2} />
            </ChessboardStoreContext.Provider>,
        );
        const evenRow = screen.getByTestId("moveRow");
        expect(evenRow).toHaveClass("bg-white/10");
    });
});
