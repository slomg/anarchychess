import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";
import { act } from "react";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import useBotVoiceLines, {
    VoiceLineContext,
} from "../../hooks/useBotVoiceLines";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { BotClientEvents, useBotEvent } from "../../hooks/useBotHub";
import { decodeMovePath } from "@/features/liveGame/lib/moveDecoder";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { PlayerType } from "@/features/liveGame/lib/types";
import { GameColor, MoveSnapshot } from "@/lib/apiClient";
import BotVoiceLines from "../BotVoiceLines";
import constants from "@/lib/constants";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";

vi.mock("../../hooks/useBotVoiceLines");
vi.mock("@/features/bot/hooks/useBotHub");

describe("BotVoiceLine", () => {
    const useBotVoiceLinesMock = vi.mocked(useBotVoiceLines);
    const useBotEventMock = vi.mocked(useBotEvent);

    const getVoiceLineForMoveMock = vi.fn();
    const getVoiceLineForGameStartMock = vi.fn();
    const getVoiceLineForGameEndMock = vi.fn();
    const botEventHandlers: EventHandlers<BotClientEvents> = {};

    let liveStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    let prevPieces: BoardPieces;

    beforeEach(() => {
        liveStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        const positionHistory = new PositionHistory(createFakeBoardPieces());
        prevPieces = createFakeBoardPieces();
        positionHistory.addNextPosition(
            createFakePositionProps({ pieces: prevPieces }),
        );
        chessboardStore.setState({ positionHistory });

        useBotVoiceLinesMock.mockReturnValue({
            getVoiceLineForMove: getVoiceLineForMoveMock,
            getVoiceLineForGameStart: getVoiceLineForGameStartMock,
            getVoiceLineForGameEnd: getVoiceLineForGameEndMock,
        });
        useBotEventMock.mockImplementation((_, event, handler) => {
            botEventHandlers[event] = handler;
        });

        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    async function renderComponent(botColor?: GameColor) {
        return render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <LiveChessStoreContext.Provider value={liveStore}>
                    <BotVoiceLines
                        botColor={botColor ?? GameColor.WHITE}
                        chessboardStore={chessboardStore}
                    />
                </LiveChessStoreContext.Provider>
            </ChessboardStoreContext.Provider>,
        );
    }

    async function FireBotMoveMade({
        moveSnapshot,
        plyNumber,
        evalForBot,
        didMoveEndGame,
    }: {
        moveSnapshot?: MoveSnapshot;
        plyNumber?: number;
        evalForBot?: number;
        didMoveEndGame?: boolean;
    } = {}): Promise<{
        moveSnapshot: MoveSnapshot;
        plyNumber: number;
        evalForBot: number;
        didMoveEndGame: boolean;
    }> {
        moveSnapshot ??= createFakeMoveSnapshot();
        plyNumber ??= 2;
        evalForBot ??= 6969;
        didMoveEndGame ??= false;

        await act(() =>
            botEventHandlers.BotMadeMoveAsync?.(
                moveSnapshot,
                plyNumber,
                "legal moves",
                evalForBot,
                didMoveEndGame,
            ),
        );

        return { moveSnapshot, plyNumber, evalForBot, didMoveEndGame };
    }

    async function FireHumanMoveMade({
        moveSnapshot,
        plyNumber,
        evalForBot,
        didMoveEndGame,
    }: {
        moveSnapshot?: MoveSnapshot;
        plyNumber?: number;
        evalForBot?: number;
        didMoveEndGame?: boolean;
    } = {}): Promise<{
        moveSnapshot: MoveSnapshot;
        plyNumber: number;
        evalForBot: number;
        didMoveEndGame: boolean;
    }> {
        moveSnapshot ??= createFakeMoveSnapshot();
        plyNumber ??= 2;
        evalForBot ??= 6969;
        didMoveEndGame ??= false;

        await act(() =>
            botEventHandlers.PlayerMadeMoveAsync?.(
                moveSnapshot,
                plyNumber,
                didMoveEndGame,
            ),
        );

        return { moveSnapshot, plyNumber, evalForBot, didMoveEndGame };
    }

    it.each([GameColor.WHITE, GameColor.BLACK])(
        "should render correctly",
        (botColor) => {
            renderComponent(botColor);

            const botPlayer = liveStore.getState().getPlayerByColor(botColor);
            expect(screen.getByTestId("profilePicture")).toHaveAttribute(
                "data-userid",
                botPlayer.userId,
            );
            expect(
                screen.queryByTestId("botVoiceLine"),
            ).not.toBeInTheDocument();
        },
    );

    it("should set voice line after bot move", async () => {
        getVoiceLineForMoveMock.mockReturnValue("test bot");

        renderComponent();
        await FireBotMoveMade();

        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine")).toHaveTextContent(
            "test bot",
        );
    });

    it("should set voice line after hunann move", async () => {
        getVoiceLineForMoveMock.mockReturnValue("test human");

        renderComponent();
        await FireHumanMoveMade();

        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine")).toHaveTextContent(
            "test human",
        );
    });

    it("should pass correct context to voice lines", async () => {
        renderComponent();

        const { moveSnapshot, plyNumber, evalForBot } = await FireBotMoveMade();

        const expectedMove = decodeMovePath(
            moveSnapshot.path,
            constants.BOARD_WIDTH,
        );
        const expectedCtx: VoiceLineContext = {
            move: expectedMove,
            prevPieces,
            plyNumber,
            playerType: PlayerType.Bot,
            evalForBot: evalForBot,
            prevEvalForBot: null,
        };

        expect(getVoiceLineForMoveMock).toHaveBeenCalledExactlyOnceWith(
            expectedCtx,
        );
    });

    it.each([PlayerType.Bot, PlayerType.Human])(
        "should pass the correct player type to voice lines",
        async (playerType) => {
            renderComponent();

            if (playerType === PlayerType.Bot) {
                await FireBotMoveMade();
            } else {
                await FireHumanMoveMade();
            }

            expect(getVoiceLineForMoveMock).toHaveBeenCalledExactlyOnceWith(
                expect.objectContaining<Partial<VoiceLineContext>>({
                    playerType,
                }),
            );
        },
    );

    it("should track prev eval for bot", async () => {
        renderComponent();

        const firstEvalForBot = 6969;
        await FireBotMoveMade({ evalForBot: firstEvalForBot });

        getVoiceLineForMoveMock.mockClear();

        const secondEvalForBot = 420420;
        await FireBotMoveMade({ evalForBot: secondEvalForBot });

        expect(getVoiceLineForMoveMock).toHaveBeenCalledExactlyOnceWith(
            expect.objectContaining<Partial<VoiceLineContext>>({
                evalForBot: secondEvalForBot,
                prevEvalForBot: firstEvalForBot,
            }),
        );
    });

    it("should fade old line out and show new line after transition", async () => {
        renderComponent();

        getVoiceLineForMoveMock.mockReturnValueOnce("first line");
        await FireBotMoveMade();

        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine").textContent).toBe(
            "first line",
        );

        getVoiceLineForMoveMock.mockReturnValueOnce("second line");
        await FireBotMoveMade();

        expect(screen.getByTestId("botVoiceLine").textContent).toBe(
            "first line",
        );

        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine").textContent).toBe(
            "second line",
        );
    });

    it("should not set a voice line if a bot move ends the game", async () => {
        renderComponent();
        getVoiceLineForMoveMock.mockReturnValue("should not appear");

        await FireBotMoveMade({ didMoveEndGame: true });
        await act(() => vi.advanceTimersByTime(300));

        expect(getVoiceLineForMoveMock).not.toHaveBeenCalled();
        expect(screen.queryByTestId("botVoiceLine")).not.toBeInTheDocument();
    });

    it("should not set a voice line if a human move ends the game", async () => {
        renderComponent();
        getVoiceLineForMoveMock.mockReturnValue("should not appear");

        await FireHumanMoveMade({ didMoveEndGame: true });
        await act(() => vi.advanceTimersByTime(300));

        expect(getVoiceLineForMoveMock).not.toHaveBeenCalled();
        expect(screen.queryByTestId("botVoiceLine")).not.toBeInTheDocument();
    });

    it("should set game start voice line when viewing first position", async () => {
        chessboardStore.setState({
            positionHistory: new PositionHistory(createFakeBoardPieces()),
        });
        getVoiceLineForGameStartMock.mockReturnValue("start line");
        getVoiceLineForMoveMock.mockReturnValueOnce("move line");

        renderComponent();
        await act(() => vi.advanceTimersByTime(300));

        expect(screen.getByTestId("botVoiceLine")).toHaveTextContent(
            "start line",
        );
        expect(getVoiceLineForGameStartMock).toHaveBeenCalledExactlyOnceWith(
            liveStore.getState().gameToken,
        );

        await act(() =>
            chessboardStore.getState().addPosition(createFakePositionProps()),
        );
        await act(() => FireBotMoveMade());
        await act(() => vi.advanceTimersByTime(300));

        expect(screen.getByTestId("botVoiceLine")).toHaveTextContent(
            "move line",
        );
    });

    it("should set game end voice line when resultData appears", async () => {
        getVoiceLineForGameEndMock.mockReturnValue("end line");

        const botColor = GameColor.BLACK;
        renderComponent(botColor);

        await act(() => vi.advanceTimersByTime(300));
        expect(screen.queryByTestId("botVoiceLine")).not.toBeInTheDocument();

        const resultData = createFakeGameResultData();
        act(() => liveStore.setState({ resultData }));
        await act(() => vi.advanceTimersByTime(300));

        expect(screen.getByTestId("botVoiceLine")).toHaveTextContent(
            "end line",
        );
        expect(getVoiceLineForGameEndMock).toHaveBeenCalledExactlyOnceWith(
            resultData.result,
            botColor,
            liveStore.getState().gameToken,
        );
    });
});
