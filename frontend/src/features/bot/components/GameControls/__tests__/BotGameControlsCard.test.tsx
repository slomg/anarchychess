import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import { BotType, GameColor } from "@/lib/apiClient";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";
import BotGameControlsCard from "../BotGameControlsCard";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";

vi.mock("@/features/bot/hooks/useBotHub");

describe("BotGameControlsCard", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createLiveChessStore(
            createFakeLiveChessStoreProps({ viewerColor: GameColor.WHITE }),
        );
    });

    it("should render live controls if viewer is playing and the game is ongoing", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotGameControlsCard botType={BotType.ANARCHY_BOT} />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByTitle("Resign")).toBeInTheDocument();
        expect(screen.queryByText("Play New Bot")).not.toBeInTheDocument();
    });

    it("should render over controls if the viewer is not playing and the game is ongoing", () => {
        store.setState({ viewer: { playerColor: null, userId: "id" } });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotGameControlsCard botType={BotType.ANARCHY_BOT} />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText("Play New Bot")).toBeInTheDocument();
        expect(screen.queryByTitle("Resign")).not.toBeInTheDocument();
    });

    it("should render over controls if the game is over", () => {
        store.setState({ resultData: createFakeGameResultData() });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotGameControlsCard botType={BotType.ANARCHY_BOT} />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText("Play New Bot")).toBeInTheDocument();
        expect(screen.queryByTitle("Resign")).not.toBeInTheDocument();
    });
});
