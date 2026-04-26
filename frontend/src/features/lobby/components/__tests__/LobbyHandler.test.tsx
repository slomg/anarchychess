import { act, render } from "@testing-library/react";
import { usePathname } from "next/navigation";

import {
    LobbyClientEvents,
    useLobbyEmitter,
    useLobbyEvent,
} from "../../hooks/useLobbyHub";

import { createFakeOngoingGame } from "@/lib/testUtils/fakers/ongoingGameFaker";
import gameStartRedirect from "@/features/liveGame/lib/gameStartRedirect";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import createFakeOpenSeek from "@/lib/testUtils/fakers/openSeekFaker";
import OpenSeekTracker from "../../lib/openSeekTracker";
import useLobbyStore from "../../stores/lobbyStore";
import { PoolKeyStr } from "../../lib/types";
import { PoolType } from "@/lib/apiClient";
import LobbyHandler from "../LobbyHandler";

vi.mock("@/features/liveGame/lib/gameStartRedirect");
vi.mock("@/features/lobby/hooks/useLobbyHub");
vi.mock("next/navigation");

describe("LobbyHandler", () => {
    const lobbyHandlers: EventHandlers<LobbyClientEvents> = {};
    const sendLobbyEventMock = vi.fn();
    const usePathnameMock = vi.mocked(usePathname);

    beforeEach(() => {
        useLobbyStore.setState(useLobbyStore.getInitialState());

        vi.mocked(useLobbyEmitter).mockReturnValue(sendLobbyEventMock);
        vi.mocked(useLobbyEvent).mockImplementation((event, handler) => {
            lobbyHandlers[event] = handler;
        });
    });

    it("should call redirect when match is found", async () => {
        const gameToken = "test game";

        render(<LobbyHandler />);
        await act(() => lobbyHandlers.MatchFoundAsync?.(gameToken));

        expect(gameStartRedirect).toHaveBeenCalledExactlyOnceWith(
            gameToken,
            expect.anything(),
        );
    });

    it("should send cleanup events and clear seeks when pathname changes and seeks are present", () => {
        const seeks = new Set<PoolKeyStr>([`${PoolType.CASUAL}-15+0`]);
        useLobbyStore.setState({ seeks });

        usePathnameMock.mockReturnValue("/path1");
        const { unmount } = render(<LobbyHandler />);

        usePathnameMock.mockReturnValue("/path2");
        unmount();
        render(<LobbyHandler />);

        expect(sendLobbyEventMock).toHaveBeenCalledExactlyOnceWith(
            "CleanupConnectionAsync",
        );
    });

    it("should clear requestedOpenSeek when pathname changes", () => {
        useLobbyStore.setState({ requestedOpenSeek: true });

        usePathnameMock.mockReturnValue("/path1");
        const { rerender } = render(<LobbyHandler />);

        usePathnameMock.mockReturnValue("/path2");
        rerender(<LobbyHandler />);

        expect(useLobbyStore.getState().requestedOpenSeek).toBe(false);
        expect(sendLobbyEventMock).toHaveBeenCalledExactlyOnceWith(
            "CleanupConnectionAsync",
        );
    });

    it("should not send cleanup event if pathname has not changed", () => {
        const seeks = new Set<PoolKeyStr>([`${PoolType.RATED}-10+5`]);
        useLobbyStore.setState({ seeks });

        usePathnameMock.mockReturnValue("/path1");
        const { unmount } = render(<LobbyHandler />);

        usePathnameMock.mockReturnValue("/path1");
        unmount();
        render(<LobbyHandler />);

        expect(sendLobbyEventMock).not.toHaveBeenCalled();
    });

    it("should send cleanup after multiple path changes", () => {
        const seeks = new Set<PoolKeyStr>([`${PoolType.RATED}-10+5`]);
        useLobbyStore.setState({ seeks });

        usePathnameMock.mockReturnValue("/path1");
        const { unmount } = render(<LobbyHandler />);

        usePathnameMock.mockReturnValue("/path2");
        unmount();
        const { rerender } = render(<LobbyHandler />);

        expect(sendLobbyEventMock).toHaveBeenCalledExactlyOnceWith(
            "CleanupConnectionAsync",
        );
        sendLobbyEventMock.mockClear();
        useLobbyStore.setState({ seeks });
        usePathnameMock.mockReturnValue("/path3");

        rerender(<LobbyHandler />);

        expect(sendLobbyEventMock).toHaveBeenCalledExactlyOnceWith(
            "CleanupConnectionAsync",
        );
    });

    it("should not send cleanup event if pathname changes but no seeks are present", () => {
        useLobbyStore.setState({ seeks: new Set() });
        usePathnameMock.mockReturnValue("/new-path");

        usePathnameMock.mockReturnValue("/path1");
        const { rerender } = render(<LobbyHandler />);

        usePathnameMock.mockReturnValue("/path2");
        rerender(<LobbyHandler />);

        expect(sendLobbyEventMock).not.toHaveBeenCalled();
    });

    it("should always clear seeks when pathname changes", () => {
        const openSeekTracker = new OpenSeekTracker();
        openSeekTracker.addSeeks([createFakeOpenSeek(), createFakeOpenSeek()]);
        useLobbyStore.setState({
            openSeekTracker,
            seeks: new Set(),
            requestedOpenSeek: false,
        });

        usePathnameMock.mockReturnValue("/path1");
        const { rerender } = render(<LobbyHandler />);

        expect(
            useLobbyStore.getState().openSeekTracker.interleavedOpenSeeks
                .length,
        ).toBe(2);

        usePathnameMock.mockReturnValue("/path2");
        rerender(<LobbyHandler />);

        expect(
            useLobbyStore.getState().openSeekTracker.interleavedOpenSeeks
                .length,
        ).toBe(0);
    });

    it("should add ongoing games when ReceiveOngoingGamesAsync is triggered", async () => {
        const games = [createFakeOngoingGame(), createFakeOngoingGame()];

        render(<LobbyHandler />);
        await act(() => lobbyHandlers.ReceiveOngoingGamesAsync?.(games));

        expect(
            Array.from(useLobbyStore.getState().ongoingGames.values()),
        ).toEqual(games);
    });

    it("should remove an ongoing game when OngoingGameEndedAsync is triggered", async () => {
        const initial = new Map([
            ["game123", createFakeOngoingGame({ gameToken: "game123" })],
            ["game456", createFakeOngoingGame({ gameToken: "game456" })],
        ]);
        useLobbyStore.setState({ ongoingGames: initial });

        render(<LobbyHandler />);
        await act(() => lobbyHandlers.OngoingGameEndedAsync?.("game456"));

        expect(useLobbyStore.getState().ongoingGames.has("game456")).toBe(
            false,
        );
        expect(useLobbyStore.getState().ongoingGames.has("game123")).toBe(true);
    });
});
