import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import { GameColor } from "@/lib/apiClient";
import { act, render, screen } from "@testing-library/react";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import GameClock from "../GameClock";
import { createFakeClocks } from "@/lib/testUtils/fakers/clocksFaker";

vi.mock("@/features/audio/audioPlayer");

describe("GameClock", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        vi.useFakeTimers();
        vi.setSystemTime(1000);
        store = createLiveChessStore(
            createFakeLiveChessStoreProps({
                clocks: createFakeClocks({
                    whiteClock: 300_000,
                    blackClock: 300_000,
                }),
                sideToMove: GameColor.WHITE,
            }),
        );
    });

    it("should render initial time correctly", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        expect(screen.getByText("05:00")).toBeInTheDocument();
    });

    it("should count down over time when active", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(10_000);
        });

        expect(screen.getByText("04:50")).toBeInTheDocument();
    });

    it("should not count down when it's not the player's turn", () => {
        store.setState({ sideToMove: GameColor.BLACK });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(10_000);
        });

        expect(screen.getByText("05:00")).toBeInTheDocument();
    });

    it("should freeze clock when isFrozen is true", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 300_000,
                blackClock: 300_000,
                isFrozen: true,
            }),
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            vi.advanceTimersByTime(10_000);
        });

        // should still show initial time because frozen
        expect(screen.getByText("05:00")).toBeInTheDocument();
    });

    it("should show decimal seconds and animate under 20s", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 15_000,
                blackClock: 300_000,
            }),
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(5000);
        });

        const clock = screen.getByText(/00:10\.\d\d/);
        expect(clock).toBeInTheDocument();
        expect(clock.classList.contains("animate-freakout")).toBe(true);
    });

    it("should apply 'text-red-600' class when clock is zero and frozen", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 0,
                blackClock: 300_000,
                isFrozen: true,
            }),
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            vi.advanceTimersByTime(1000);
        });

        const clock = screen.getByText("00:00.00");
        expect(clock.classList.contains("text-red-600")).toBe(true);
        expect(clock.classList.contains("animate-freakout")).toBe(false);
    });

    it("should show zero and doesn't go negative", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 5000,
                blackClock: 300_000,
            }),
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(7000);
        });

        expect(screen.getByText("00:00.00")).toBeInTheDocument();
    });

    it("should stop ticking when isFrozen is true", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 1000,
                blackClock: 1000,
                isFrozen: true,
            }),
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(10000);
        });

        expect(screen.getByText("00:01.00")).toBeInTheDocument();
    });

    it("should apply increment to the clock when turn changes", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 300_000,
                blackClock: 300_000,
            }),
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            vi.advanceTimersByTime(5000);
        });

        expect(screen.getByText("04:55")).toBeInTheDocument();

        act(() => {
            store.setState({
                sideToMove: GameColor.BLACK,
                clocks: {
                    ...store.getState().clocks,
                    whiteClock: 305_000,
                    lastUpdated: Date.now().valueOf(),
                },
            });
        });

        expect(screen.getByText("05:05")).toBeInTheDocument();
    });

    it("should play warning sound once when time goes under 20s", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 21_000,
                blackClock: 300_000,
            }),
            sideToMove: GameColor.WHITE,
            viewer: { playerColor: GameColor.WHITE, userId: "id" },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();

        act(() => {
            vi.advanceTimersByTime(2000);
        });

        expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
            AudioType.LOW_TIME,
        );
    });

    it("should not play sound if viewer is not the player", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 15_000,
                blackClock: 300_000,
            }),
            sideToMove: GameColor.WHITE,
            viewer: { playerColor: GameColor.BLACK, userId: "id" }, // not same color
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();
    });

    it("should not play sound when clock is frozen", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 15_000,
                blackClock: 300_000,
                isFrozen: true,
            }),
            sideToMove: GameColor.WHITE,
            viewer: { playerColor: GameColor.WHITE, userId: "id" },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();
    });

    it("should account for server clock ahead ", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 60_000,
                blackClock: 60_000,
            }),
            serverClockAheadByMs: 5_000,
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            vi.advanceTimersByTime(1_000);
        });

        // elapsed = 1s local + 5s drift = 6s
        // time left = 60 - 6 = 54s
        expect(screen.getByText("00:54")).toBeInTheDocument();
    });

    it("should account for server clock behind", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 60_000,
                blackClock: 60_000,
            }),
            serverClockAheadByMs: -3_000,
            sideToMove: GameColor.WHITE,
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            vi.advanceTimersByTime(2_000);
        });

        // elapsed = 2s local + (-3s drift) = -1s
        // time left = 60 - (-1) = 61s
        expect(screen.getByText("01:01")).toBeInTheDocument();
    });

    it("should initialize time accounting for server clock drift", () => {
        store.setState({
            clocks: createFakeClocks({
                whiteClock: 60_000,
                blackClock: 60_000,
            }),
            serverClockAheadByMs: 5_000,
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText("00:55")).toBeInTheDocument();
    });
});
