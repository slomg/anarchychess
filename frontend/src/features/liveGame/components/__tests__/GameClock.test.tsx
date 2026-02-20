import { StoreApi } from "zustand";
import { act, render, screen } from "@testing-library/react";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";
import { createFakeClockPlayer } from "@/lib/testUtils/fakers/createFakeClockPlayer";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { createFakeClocks } from "@/lib/testUtils/fakers/clocksFaker";
import { GameColor } from "@/lib/apiClient";
import GameClock from "../GameClock";

vi.mock("@/features/audio/audioPlayer");

describe("GameClock", () => {
    let liveStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    beforeEach(() => {
        vi.useFakeTimers();
        vi.setSystemTime(1000);

        liveStore = createLiveChessStore(
            createFakeLiveChessStoreProps({
                liveClocks: createFakeClocks({
                    whiteClock: createFakeClockPlayer({ timeLeftMs: 300_000 }),
                    blackClock: createFakeClockPlayer({ timeLeftMs: 300_000 }),
                }),
                sideToMove: GameColor.WHITE,
            }),
        );
        chessboardStore = createChessboardStore();
    });

    function renderWithCtx(color: GameColor) {
        return render(
            <LiveChessStoreContext.Provider value={liveStore}>
                <ChessboardStoreContext.Provider value={chessboardStore}>
                    <GameClock color={color} />
                </ChessboardStoreContext.Provider>
            </LiveChessStoreContext.Provider>,
        );
    }

    it("should render initial time correctly", () => {
        renderWithCtx(GameColor.WHITE);
        expect(screen.getByText("05:00")).toBeInTheDocument();
    });

    it("should count down over time when active", () => {
        renderWithCtx(GameColor.WHITE);
        act(() => {
            vi.advanceTimersByTime(10_000);
        });

        expect(screen.getByText("04:50")).toBeInTheDocument();
    });

    it("should not count down when it's not the player's turn", () => {
        liveStore.setState({ sideToMove: GameColor.BLACK });

        renderWithCtx(GameColor.WHITE);
        act(() => {
            vi.advanceTimersByTime(10_000);
        });

        expect(screen.getByText("05:00")).toBeInTheDocument();
    });

    it("should freeze clock when isFrozen is true", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 300_000 }),
                isFrozen: true,
            }),
        });

        renderWithCtx(GameColor.WHITE);

        act(() => {
            vi.advanceTimersByTime(10_000);
        });

        // should still show initial time because frozen
        expect(screen.getByText("05:00")).toBeInTheDocument();
    });

    it("should show decimal seconds and animate under 20s", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 15_000 }),
            }),
        });

        renderWithCtx(GameColor.WHITE);
        act(() => {
            vi.advanceTimersByTime(5000);
        });

        const clock = screen.getByText(/00:10\.\d\d/);
        expect(clock).toBeInTheDocument();
        expect(clock.classList.contains("animate-freakout")).toBe(true);
    });

    it("should apply 'text-red-600' class when clock is zero and frozen", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 0 }),
                isFrozen: true,
            }),
        });

        renderWithCtx(GameColor.WHITE);

        act(() => {
            vi.advanceTimersByTime(1000);
        });

        const clock = screen.getByText("OVERTIME");
        expect(clock.classList.contains("text-red-600")).toBe(true);
        expect(clock.classList.contains("animate-freakout")).toBe(false);
    });

    it("should show OVERTIME once time is over", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 5000 }),
            }),
        });

        renderWithCtx(GameColor.WHITE);
        act(() => {
            vi.advanceTimersByTime(7000);
        });

        expect(screen.getByText("OVERTIME")).toBeInTheDocument();
    });

    it("should stop ticking when isFrozen is true", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 1000 }),
                isFrozen: true,
            }),
        });

        renderWithCtx(GameColor.WHITE);
        act(() => {
            vi.advanceTimersByTime(10000);
        });

        expect(screen.getByText("00:01.00")).toBeInTheDocument();
    });

    it("should apply increment to the clock when turn changes", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 300_000 }),
            }),
        });

        renderWithCtx(GameColor.WHITE);

        act(() => {
            vi.advanceTimersByTime(5000);
        });

        expect(screen.getByText("04:55")).toBeInTheDocument();

        act(() => {
            liveStore.setState({
                sideToMove: GameColor.BLACK,
                liveClocks: {
                    ...liveStore.getState().liveClocks!,
                    whiteClock: createFakeClockPlayer({ timeLeftMs: 305_000 }),
                    lastUpdated: Date.now().valueOf(),
                },
            });
        });

        expect(screen.getByText("05:05")).toBeInTheDocument();
    });

    it("should play warning sound once when time goes under 20s", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 21_000 }),
            }),
            sideToMove: GameColor.WHITE,
            viewer: { playerColor: GameColor.WHITE, userId: "id" },
        });

        renderWithCtx(GameColor.WHITE);

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();

        act(() => {
            vi.advanceTimersByTime(2000);
        });

        expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
            AudioType.LOW_TIME,
        );
    });

    it("should not play sound if viewer is not the player", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 15_000 }),
            }),
            sideToMove: GameColor.WHITE,
            viewer: { playerColor: GameColor.BLACK, userId: "id" }, // not same color
        });

        renderWithCtx(GameColor.WHITE);

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();
    });

    it("should not play sound when clock is frozen", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 15_000 }),
                isFrozen: true,
            }),
            sideToMove: GameColor.WHITE,
            viewer: { playerColor: GameColor.WHITE, userId: "id" },
        });

        renderWithCtx(GameColor.WHITE);

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();
    });

    it("should account for server clock ahead ", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 60_000 }),
            }),
            serverClockAheadByMs: 5_000,
        });

        renderWithCtx(GameColor.WHITE);

        act(() => {
            vi.advanceTimersByTime(1_000);
        });

        // elapsed = 1s local + 5s drift = 6s
        // time left = 60 - 6 = 54s
        expect(screen.getByText("00:54")).toBeInTheDocument();
    });

    it("should account for server clock behind", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 60_000 }),
            }),
            serverClockAheadByMs: -3_000,
        });

        renderWithCtx(GameColor.WHITE);

        act(() => {
            vi.advanceTimersByTime(2_000);
        });

        // elapsed = 2s local + (-3s drift) = -1s
        // time left = 60 - (-1) = 61s
        expect(screen.getByText("01:01")).toBeInTheDocument();
    });

    it("should initialize time accounting for server clock drift", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({ timeLeftMs: 60_000 }),
            }),
            serverClockAheadByMs: 5_000,
        });

        renderWithCtx(GameColor.WHITE);

        expect(screen.getByText("00:55")).toBeInTheDocument();
    });

    it("should render timeUntilAbandonedMs when ticking", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({
                    timeLeftMs: 300_000,
                    timeUntilAbandonMs: 10_000,
                }),
            }),
        });

        renderWithCtx(GameColor.WHITE);

        expect(screen.getByText("move in 10s")).toBeInTheDocument();
    });

    it("should decrement timeUntilAbandonedMs as time passes", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({
                    timeLeftMs: 300_000,
                    timeUntilAbandonMs: 10_000,
                }),
            }),
        });

        renderWithCtx(GameColor.WHITE);

        act(() => {
            vi.advanceTimersByTime(3_000);
        });

        expect(screen.getByText("move in 7s")).toBeInTheDocument();
    });

    it("should not render timeUntilAbandonedMs when it is null", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({
                    timeLeftMs: 300_000,
                    timeUntilAbandonMs: null,
                }),
            }),
        });

        renderWithCtx(GameColor.WHITE);

        expect(screen.queryByText(/move in \d+s/)).toBeNull();
    });

    it("should not render timeUntilAbandonedMs when not ticking", () => {
        liveStore.setState({
            liveClocks: createFakeClocks({
                whiteClock: createFakeClockPlayer({
                    timeLeftMs: 300_000,
                    timeUntilAbandonMs: 10_000,
                }),
            }),
            sideToMove: GameColor.BLACK,
        });

        renderWithCtx(GameColor.WHITE);

        expect(screen.queryByText(/move in \d+s/)).toBeNull();
    });

    it("should render the snapshot clock for a past ply", () => {
        liveStore.setState({
            resultData: createFakeGameResultData(),
            clockSnapshotByPly: new Map([
                [2, { whiteClock: 250_000, blackClock: 245_000 }],
            ]),
        });

        const positionHistory = new PositionHistory(createFakeBoardPieces());
        positionHistory.addNextPosition(createFakePositionProps());
        const pos2 = positionHistory.addNextPosition(createFakePositionProps());
        positionHistory.addNextPosition(createFakePositionProps());
        positionHistory.goToPosition(pos2.positionId);
        chessboardStore.setState({ positionHistory });

        renderWithCtx(GameColor.WHITE);

        expect(screen.getByText("04:10")).toBeInTheDocument(); // 250,000ms -> 4:10
    });

    it("should update the clock when switching plies", async () => {
        liveStore.setState({
            resultData: createFakeGameResultData(),
            clockSnapshotByPly: new Map([
                [1, { whiteClock: 290_000, blackClock: 295_000 }],
                [2, { whiteClock: 250_000, blackClock: 245_000 }],
            ]),
        });

        const positionHistory = new PositionHistory(createFakeBoardPieces());
        const pos1 = positionHistory.addNextPosition(createFakePositionProps());
        const pos2 = positionHistory.addNextPosition(createFakePositionProps());
        positionHistory.addNextPosition(createFakePositionProps());
        positionHistory.goToPosition(pos1.positionId);
        chessboardStore.setState({ positionHistory });

        renderWithCtx(GameColor.WHITE);
        expect(screen.getByText("04:50")).toBeInTheDocument();

        await act(() =>
            chessboardStore.getState().goToPosition(pos2.positionId),
        );

        expect(screen.getByText("04:10")).toBeInTheDocument();
    });

    it("should not render anything if liveClocks is null", () => {
        liveStore.setState({ liveClocks: null });

        const { container } = renderWithCtx(GameColor.WHITE);
        expect(container).toBeEmptyDOMElement();
    });
});
