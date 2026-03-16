import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";

import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { mockRouter, RouterMock } from "@/lib/testUtils/mocks/mockRouter";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";
import BotGameOverPopup from "../BotGameOverPopup";
import useBotMatch from "../../hooks/useBotMatch";
import { BotType, GameColor } from "@/lib/apiClient";
import constants from "@/lib/constants";

vi.mock("@/features/bot/hooks/useBotMatch");

describe("BotGameOverPopup", () => {
    let store: StoreApi<LiveChessStore>;
    let routerMock: RouterMock;
    const useBotMatchMock = vi.mocked(useBotMatch);
    const matchBotGameMock = vi.fn();

    beforeEach(() => {
        store = createLiveChessStore(
            createFakeLiveChessStoreProps({ viewerColor: GameColor.WHITE }),
        );
        store.setState({ resultData: createFakeGameResultData() });

        routerMock = mockRouter();
        useBotMatchMock.mockReturnValue({
            matchBotGame: matchBotGameMock,
            isMatching: false,
        });
    });

    it("should only render play anarchy bot button if viewer not in game", () => {
        store.setState({ viewer: { playerColor: null, userId: "user" } });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotGameOverPopup botType={BotType.ANARCHY_BOT} />
            </LiveChessStoreContext.Provider>,
        );

        expect(
            screen.getByRole("button", { name: /PLAY ANARCHY BOT/i }),
        ).toBeInTheDocument();
        expect(
            screen.queryByRole("button", { name: /REMATCH/i }),
        ).not.toBeInTheDocument();
        expect(
            screen.queryByRole("button", { name: /PLAY NEW BOT/i }),
        ).not.toBeInTheDocument();
    });

    it("should render both play new bot and rematch buttons if viewer is in game", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotGameOverPopup botType={BotType.ANARCHY_BOT} />
            </LiveChessStoreContext.Provider>,
        );

        expect(
            screen.getByRole("button", { name: /PLAY NEW BOT/i }),
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: /REMATCH/i }),
        ).toBeInTheDocument();
    });

    it("should navigate to bot page when play new bot is clicked", async () => {
        const user = userEvent.setup();
        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotGameOverPopup botType={BotType.ANARCHY_BOT} />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(screen.getByRole("button", { name: /PLAY NEW BOT/i }));

        expect(routerMock.push).toHaveBeenCalledExactlyOnceWith(
            constants.PATHS.BOT,
        );
    });

    it("should start a game with inverse color when rematch is clicked", async () => {
        const user = userEvent.setup();
        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotGameOverPopup botType={BotType.ANARCHY_BOT} />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(screen.getByRole("button", { name: /REMATCH/i }));

        expect(matchBotGameMock).toHaveBeenCalledExactlyOnceWith(
            GameColor.BLACK,
            BotType.ANARCHY_BOT,
        );
    });

    it.each([BotType.ANARCHY_BOT, BotType.LOBOTOMIZED_ANARCHY_BOT])(
        "should start a game with the correct bot type",
        async (botType) => {
            const user = userEvent.setup();
            render(
                <LiveChessStoreContext.Provider value={store}>
                    <BotGameOverPopup botType={botType} />
                </LiveChessStoreContext.Provider>,
            );

            await user.click(screen.getByRole("button", { name: /REMATCH/i }));

            expect(matchBotGameMock).toHaveBeenCalledExactlyOnceWith(
                GameColor.BLACK,
                botType,
            );
        },
    );

    it("should start a game with null color if viewer not in game", async () => {
        const user = userEvent.setup();
        store.setState({ viewer: { playerColor: null, userId: "user" } });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <BotGameOverPopup botType={BotType.ANARCHY_BOT} />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(
            screen.getByRole("button", { name: /PLAY ANARCHY BOT/i }),
        );

        expect(matchBotGameMock).toHaveBeenCalledExactlyOnceWith(
            null,
            BotType.ANARCHY_BOT,
        );
    });
});
