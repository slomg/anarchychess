import { render, screen, within } from "@testing-library/react";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import PositionHistory, {
    Position,
} from "@/features/chessboard/lib/positionHistory";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import { StoreApi } from "zustand";
import MoveVariation from "../MoveVariation";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { createFakePositionHistory } from "@/lib/testUtils/fakers/positionHistoryFaker";
import userEvent from "@testing-library/user-event";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";
import BoardPieces from "@/features/chessboard/lib/boardPieces";

describe("MoveVariation", () => {
    let rootPieces: BoardPieces;
    let history: PositionHistory;
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        rootPieces = createFakeBoardPieces();
        history = new PositionHistory(rootPieces);
        chessboardStore = createChessboardStore();
        chessboardStore.setState({ positionHistory: history });
    });

    function renderWithCtx(variations: readonly Position[]) {
        return render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <MoveVariation variations={variations} />
            </ChessboardStoreContext.Provider>,
        );
    }

    it("renders a single mainline correctly", () => {
        /**
         * └ 1.e4 e5 2.Nf3
         */

        const pos1 = history.addNextPosition(
            createFakePositionProps({ san: "e4" }),
        );
        history.addNextPosition(createFakePositionProps({ san: "e5" }));
        history.addNextPosition(
            createFakePositionProps({
                san: "Nf3",
            }),
        );

        renderWithCtx([pos1]);

        const moveVariations = screen.getByTestId("moveVariations");
        expect(moveVariations).toHaveTextContent(
            "1.e4 e5 2.Nf3".replaceAll(" ", ""),
        );
    });

    it("renders multi level variations correctly", () => {
        /**
         * ├ 1.e4 e5
         * |   ├ 2.Nf3
         * |   ├ 2.Nc3
         * |   |   ├ 2...Nf6
         * |   |   └ 2...Nc6
         * |   └ 2.d4
         */

        const e4 = history.addNextPosition(
            createFakePositionProps({ san: "e4" }),
        );
        const e5 = history.addNextPosition(
            createFakePositionProps({ san: "e5" }),
        );
        history.addNextPosition(createFakePositionProps({ san: "Nf3" }));

        history.goToPosition(e5.positionId);
        const nc3 = history.addNextPosition(
            createFakePositionProps({
                san: "Nc3",
            }),
        );
        history.addNextPosition(
            createFakePositionProps({
                san: "Nf6",
            }),
        );
        history.goToPosition(nc3.positionId);
        history.addNextPosition(
            createFakePositionProps({
                san: "Nc6",
            }),
        );

        history.goToPosition(e5.positionId);
        history.addNextPosition(
            createFakePositionProps({
                san: "d4",
            }),
        );

        const { debug } = renderWithCtx([e4]);
        debug();

        const rootVariation = screen.getAllByTestId("moveVariations")[0];

        const rootLines = [...rootVariation.children].filter(
            (x) => (x as HTMLElement).dataset.testid === "lineVariation",
        );
        expect(rootLines).toHaveLength(1);
        expect(rootLines[0]).toHaveTextContent("1.e4 e5".replaceAll(" ", ""));

        const firstLevelVariations = [...rootVariation.children].filter(
            (x) => (x as HTMLElement).dataset.testid === "moveVariations",
        );
        expect(firstLevelVariations).toHaveLength(1);
        const firstLevelVariation = firstLevelVariations[0];

        const firstLevelLines = [...firstLevelVariation.children].filter(
            (x) => (x as HTMLElement).dataset.testid === "lineVariation",
        );
        expect(firstLevelLines).toHaveLength(3);
        expect(firstLevelLines[0]).toHaveTextContent("2.Nf3");
        expect(firstLevelLines[1]).toHaveTextContent("2.Nc3");
        expect(firstLevelLines[2]).toHaveTextContent("2.d4");

        const nc3VariationContainers = [...firstLevelVariation.children].filter(
            (x) => (x as HTMLElement).dataset.testid === "moveVariations",
        );
        expect(nc3VariationContainers).toHaveLength(1);

        const nc3NestedLines = [...nc3VariationContainers[0].children].filter(
            (x) => (x as HTMLElement).dataset.testid === "lineVariation",
        );
        expect(nc3NestedLines).toHaveLength(2);
        expect(nc3NestedLines[0]).toHaveTextContent("2...Nf6");
        expect(nc3NestedLines[1]).toHaveTextContent("2...Nc6");
    });

    it("highlights selected move correctly", async () => {
        const pos1 = history.addNextPosition(
            createFakePositionProps({ san: "e4" }),
        );
        const pos2 = history.addNextPosition(
            createFakePositionProps({ san: "e5" }),
        );

        renderWithCtx([pos1]);

        const user = userEvent.setup();
        const e4Button = screen.getByText("1.e4");
        const e5Button = screen.getByText("e5");

        // Initially nothing selected
        expect(e4Button).not.toHaveClass("bg-blue-300/30");

        await user.click(e4Button);
        expect(chessboardStore.getState().positionHistory.viewingPosition).toBe(
            pos1,
        );
        expect(e4Button).toHaveClass("bg-blue-300/30");
        expect(e5Button).not.toHaveClass("bg-blue-300/30");

        await user.click(e5Button);
        expect(chessboardStore.getState().positionHistory.viewingPosition).toBe(
            pos2,
        );
        expect(e5Button).toHaveClass("bg-blue-300/30");
        expect(e4Button).not.toHaveClass("bg-blue-300/30");
    });
});
