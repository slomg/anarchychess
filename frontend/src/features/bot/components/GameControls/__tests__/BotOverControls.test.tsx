import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { mockRouter, RouterMock } from "@/lib/testUtils/mocks/mockRouter";
import useBotMatch from "@/features/bot/hooks/useBotMatch";
import BotOverControls from "../BotOverControls";
import { GameColor } from "@/lib/apiClient";
import constants from "@/lib/constants";

vi.mock("@/features/bot/hooks/useBotMatch");

describe("BotOverControls", () => {
    let store: StoreApi<LiveChessStore>;

    let routerMock: RouterMock;
    const useBotMatchMock = vi.mocked(useBotMatch);
    const matchBotGameMock = vi.fn();

    beforeEach(() => {
        store = createLiveChessStore(
            createFakeLiveChessStoreProps({ viewerColor: GameColor.WHITE }),
        );

        routerMock = mockRouter();
        useBotMatchMock.mockReturnValue({
            matchBotGame: matchBotGameMock,
            isMatching: false,
        });
    });

    it("should only render play new bot button if viewer not in game", () => {
        store.setState({ viewer: { playerColor: null, userId: "user" } });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotOverControls />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByTestId("botOverControlsNewGame")).toHaveTextContent(
            "Play New Bot",
        );
        expect(
            screen.queryByTestId("botOverControlsRematch"),
        ).not.toBeInTheDocument();
    });

    it("should render both play new bot button and rematch if viewer is in game", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotOverControls />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByTestId("botOverControlsNewGame")).toHaveTextContent(
            "Play New Bot",
        );
        expect(screen.getByTestId("botOverControlsRematch")).toHaveTextContent(
            "Rematch",
        );
    });

    it("should redirect to bots page if paly new bot is clicked", async () => {
        const user = userEvent.setup();
        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotOverControls />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(screen.getByTestId("botOverControlsNewGame"));

        expect(routerMock.push).toHaveBeenCalledExactlyOnceWith(
            constants.PATHS.BOT,
        );
    });

    it("should start a game with the inverse color when rematch is clicked", async () => {
        const user = userEvent.setup();
        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotOverControls />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(screen.getByTestId("botOverControlsRematch"));

        expect(matchBotGameMock).toHaveBeenCalledExactlyOnceWith(
            GameColor.BLACK,
        );
    });
});
