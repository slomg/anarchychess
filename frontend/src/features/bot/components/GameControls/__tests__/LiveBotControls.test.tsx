import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import { GameColor } from "@/lib/apiClient";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";
import LiveBotControls from "../LiveBotControls";
import { useBotEmitter } from "@/features/bot/hooks/useBotHub";
import userEvent from "@testing-library/user-event";

vi.mock("@/features/bot/hooks/useBotHub");

describe("LiveBotControls", () => {
    let store: StoreApi<LiveChessStore>;

    const useBotEmitterMock = vi.mocked(useBotEmitter);
    const sendBotEventMock = vi.fn();

    beforeEach(() => {
        store = createLiveChessStore(
            createFakeLiveChessStoreProps({ viewerColor: GameColor.WHITE }),
        );

        useBotEmitterMock.mockReturnValue(sendBotEventMock);
    });

    it("should render resign button", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <LiveBotControls />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByTitle("Resign")).toBeInTheDocument();
    });

    it("should resign when resign is clicked", async () => {
        const user = userEvent.setup();
        render(
            <LiveChessStoreContext.Provider value={store}>
                <LiveBotControls />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(screen.getByTitle("Resign"));
        await user.click(screen.getByTitle("Resign"));

        expect(sendBotEventMock).toHaveBeenCalledExactlyOnceWith(
            "ResignAsync",
            store.getState().gameToken,
        );
    });
});
