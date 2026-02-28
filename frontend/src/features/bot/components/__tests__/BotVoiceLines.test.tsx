import { StoreApi } from "zustand";

import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { BotClientEvents, useBotEvent } from "../../hooks/useBotHub";
import useBotVoiceLines, {
    VoiceLineContext,
} from "../../hooks/useBotVoiceLines";
import { render, screen } from "@testing-library/react";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import BotVoiceLines from "../BotVoiceLines";
import { GameColor, MoveSnapshot } from "@/lib/apiClient";
import { act } from "react";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { decodeMovePath } from "@/features/liveGame/lib/moveDecoder";
import constants from "@/lib/constants";
import { PlayerType } from "@/features/liveGame/lib/types";

vi.mock("../../hooks/useBotVoiceLines");
vi.mock("@/features/bot/hooks/useBotHub");

describe("BotVoiceLine", () => {
    const useBotVoiceLinesMock = vi.mocked(useBotVoiceLines);
    const useBotEventMock = vi.mocked(useBotEvent);

    const getVoiceLineMock = vi.fn();
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

        useBotVoiceLinesMock.mockReturnValue(getVoiceLineMock);
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
    }: {
        moveSnapshot?: MoveSnapshot;
        plyNumber?: number;
        evalForBot?: number;
    } = {}): Promise<{
        moveSnapshot: MoveSnapshot;
        plyNumber: number;
        evalForBot: number;
    }> {
        moveSnapshot ??= createFakeMoveSnapshot();
        plyNumber ??= 2;
        evalForBot ??= 6969;

        await act(() =>
            botEventHandlers.BotMadeMoveAsync?.(
                moveSnapshot,
                plyNumber,
                "legal moves",
                evalForBot,
            ),
        );

        return { moveSnapshot, plyNumber, evalForBot };
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
        getVoiceLineMock.mockReturnValue("test bot");

        renderComponent();
        await FireBotMoveMade();

        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine")).toHaveTextContent(
            "test bot",
        );
    });

    it("should set voice line after hunann move", async () => {
        getVoiceLineMock.mockReturnValue("test human");

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

        expect(getVoiceLineMock).toHaveBeenCalledExactlyOnceWith(expectedCtx);
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

            expect(getVoiceLineMock).toHaveBeenCalledExactlyOnceWith(
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

        getVoiceLineMock.mockClear();

        const secondEvalForBot = 420420;
        await FireBotMoveMade({ evalForBot: secondEvalForBot });

        expect(getVoiceLineMock).toHaveBeenCalledExactlyOnceWith(
            expect.objectContaining<Partial<VoiceLineContext>>({
                evalForBot: secondEvalForBot,
                prevEvalForBot: firstEvalForBot,
            }),
        );
    });

    it("should fade old line out and show new line after transition", async () => {
        renderComponent();

        getVoiceLineMock.mockReturnValueOnce("first line");
        await FireBotMoveMade();

        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine").textContent).toBe(
            "first line",
        );

        getVoiceLineMock.mockReturnValueOnce("second line");
        await FireBotMoveMade();

        expect(screen.getByTestId("botVoiceLine").textContent).toBe(
            "first line",
        );

        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine").textContent).toBe(
            "second line",
        );
    });

    it("should change voice line when viewing a different position", async () => {
        renderComponent();

        const { addPosition, goToPosition } = chessboardStore.getState();
        const position1 = await act(() =>
            addPosition(createFakePositionProps()),
        );
        getVoiceLineMock.mockReturnValueOnce("first line");
        await FireBotMoveMade({ plyNumber: position1.ply });

        const position2 = await act(() =>
            addPosition(createFakePositionProps()),
        );
        getVoiceLineMock.mockReturnValueOnce(null);
        await FireBotMoveMade({ plyNumber: position2.ply });

        const position3 = await act(() =>
            addPosition(createFakePositionProps()),
        );
        getVoiceLineMock.mockReturnValueOnce("second line");
        await FireBotMoveMade({ plyNumber: position3.ply });
        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine").textContent).toBe(
            "second line",
        );

        await act(() => goToPosition(position1.positionId));
        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine").textContent).toBe(
            "first line",
        );

        await act(() => goToPosition(position2.positionId));
        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine").textContent).toBe(
            "first line",
        );

        await act(() => goToPosition(position3.positionId));
        await act(() => vi.advanceTimersByTime(300));
        expect(screen.getByTestId("botVoiceLine").textContent).toBe(
            "second line",
        );
    });
});
