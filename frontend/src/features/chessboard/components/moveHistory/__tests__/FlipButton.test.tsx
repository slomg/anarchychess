import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { GameColor } from "@/lib/apiClient";
import FlipButton from "../FlipButton";

describe("FlipButton", () => {
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        chessboardStore = createChessboardStore();
    });

    it("should flip the board when the flip board icon is clicked", async () => {
        chessboardStore.setState({ viewingFrom: GameColor.WHITE });
        const user = userEvent.setup();

        render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <FlipButton />
            </ChessboardStoreContext.Provider>,
        );

        const flipIcon = screen.getByTitle("Flip Board");
        await user.click(flipIcon);

        expect(chessboardStore.getState().viewingFrom).toBe(GameColor.BLACK);
    });
});
