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
import useBotDialog, { DialogContext } from "../../hooks/useBotDialog";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import { BotClientEvents, useBotEvent } from "../../hooks/useBotHub";
import { decodeMovePath } from "@/features/liveGame/lib/moveDecoder";
import BotDialog, { BOT_DIALOG_TYPING_SPEED_MS } from "../BotDialog";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { PlayerType } from "@/features/liveGame/lib/types";
import { GameColor, MoveSnapshot } from "@/lib/apiClient";
import constants from "@/lib/constants";

vi.mock("../../hooks/useBotDialog");
vi.mock("@/features/bot/hooks/useBotHub");

describe("BotDialog", () => {
    const useBotDialogsMock = vi.mocked(useBotDialog);
    const useBotEventMock = vi.mocked(useBotEvent);

    const getDialogForMoveMock = vi.fn();
    const getDialogForGameStartMock = vi.fn();
    const getDialogForGameEndMock = vi.fn();
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

        useBotDialogsMock.mockReturnValue({
            getDialogForMove: getDialogForMoveMock,
            getDialogForGameStart: getDialogForGameStartMock,
            getDialogForGameEnd: getDialogForGameEndMock,
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
                    <BotDialog
                        botColor={botColor ?? GameColor.WHITE}
                        chessboardStore={chessboardStore}
                    />
                </LiveChessStoreContext.Provider>
            </ChessboardStoreContext.Provider>,
        );
    }

    async function fireBotMoveMade({
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

    async function fireHumanMoveMade({
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

    async function assertDialog(expectedDialog: string) {
        await act(() =>
            vi.advanceTimersByTime(
                300 + expectedDialog.length * BOT_DIALOG_TYPING_SPEED_MS,
            ),
        );
        expect(screen.getByTestId("botDialog")).toHaveTextContent(
            expectedDialog,
        );
    }

    async function assertNoDialog(fakeDialog?: string) {
        await act(() =>
            vi.advanceTimersByTime(
                300 + (fakeDialog?.length ?? 0) * BOT_DIALOG_TYPING_SPEED_MS,
            ),
        );
        expect(screen.queryByTestId("botDialog")).not.toBeInTheDocument();
    }

    it.each([GameColor.WHITE, GameColor.BLACK])(
        "should render correctly",
        async (botColor) => {
            renderComponent(botColor);

            const botPlayer = liveStore.getState().getPlayerByColor(botColor);
            expect(screen.getByTestId("profilePicture")).toHaveAttribute(
                "data-userid",
                botPlayer.userId,
            );
            await assertNoDialog();
        },
    );

    it("should set dialog after bot move", async () => {
        getDialogForMoveMock.mockReturnValue("test bot");

        renderComponent();
        await fireBotMoveMade();

        await assertDialog("test bot");
    });

    it("should set dialog after human move", async () => {
        getDialogForMoveMock.mockReturnValue("test human");

        renderComponent();
        await fireHumanMoveMade();

        await assertDialog("test human");
    });

    it("should pass correct context to dialog", async () => {
        renderComponent();

        const { moveSnapshot, plyNumber, evalForBot } = await fireBotMoveMade();

        const expectedMove = decodeMovePath(
            moveSnapshot.path,
            constants.BOARD_WIDTH,
        );
        const expectedCtx: DialogContext = {
            move: expectedMove,
            prevPieces,
            plyNumber,
            playerType: PlayerType.Bot,
            evalForBot: evalForBot,
            prevEvalForBot: null,
        };

        expect(getDialogForMoveMock).toHaveBeenCalledExactlyOnceWith(
            expectedCtx,
        );
    });

    it.each([PlayerType.Bot, PlayerType.Human])(
        "should pass the correct player type to dialog",
        async (playerType) => {
            renderComponent();

            if (playerType === PlayerType.Bot) {
                await fireBotMoveMade();
            } else {
                await fireHumanMoveMade();
            }

            expect(getDialogForMoveMock).toHaveBeenCalledExactlyOnceWith(
                expect.objectContaining<Partial<DialogContext>>({
                    playerType,
                }),
            );
        },
    );

    it("should track prev eval for bot", async () => {
        renderComponent();

        const firstEvalForBot = 6969;
        await fireBotMoveMade({ evalForBot: firstEvalForBot });

        getDialogForMoveMock.mockClear();

        const secondEvalForBot = 420420;
        await fireBotMoveMade({ evalForBot: secondEvalForBot });

        expect(getDialogForMoveMock).toHaveBeenCalledExactlyOnceWith(
            expect.objectContaining<Partial<DialogContext>>({
                evalForBot: secondEvalForBot,
                prevEvalForBot: firstEvalForBot,
            }),
        );
    });

    it("should fade in before typing starts", async () => {
        getDialogForMoveMock.mockReturnValue("fade test");
        renderComponent();

        await fireBotMoveMade();

        expect(screen.queryByTestId("botDialog")).not.toBeInTheDocument();

        await act(() =>
            vi.advanceTimersByTime(300 + BOT_DIALOG_TYPING_SPEED_MS),
        );

        expect(screen.getByTestId("botDialog")).toHaveTextContent("f"); // first letter only
    });

    it("should type out the dialog character by character", async () => {
        getDialogForMoveMock.mockReturnValue("typing");

        renderComponent();
        await fireBotMoveMade();

        await act(() => vi.advanceTimersByTime(300));

        const dialogNode = screen.getByTestId("botDialog");

        for (let i = 1; i <= "typing".length; i++) {
            await act(() => vi.advanceTimersByTime(BOT_DIALOG_TYPING_SPEED_MS));
            expect(dialogNode).toHaveTextContent("typing".slice(0, i));
        }

        expect(dialogNode).toHaveTextContent("typing");
    });

    it("should not set a dialog if a bot move ends the game", async () => {
        renderComponent();
        getDialogForMoveMock.mockReturnValue("should not appear");

        await fireBotMoveMade({ didMoveEndGame: true });

        await assertNoDialog("should not appear");
        expect(getDialogForMoveMock).not.toHaveBeenCalled();
    });

    it("should not set a dialog if a human move ends the game", async () => {
        renderComponent();
        getDialogForMoveMock.mockReturnValue("should not appear");

        await fireHumanMoveMade({ didMoveEndGame: true });

        await assertNoDialog("should not appear");
        expect(getDialogForMoveMock).not.toHaveBeenCalled();
    });

    it("should set game start dialog when viewing first position", async () => {
        chessboardStore.setState({
            positionHistory: new PositionHistory(createFakeBoardPieces()),
        });
        getDialogForGameStartMock.mockReturnValue("start line");
        getDialogForMoveMock.mockReturnValueOnce("move line");

        renderComponent();

        await assertDialog("start line");
        expect(getDialogForGameStartMock).toHaveBeenCalledExactlyOnceWith(
            liveStore.getState().gameToken,
        );

        await act(() =>
            chessboardStore.getState().addPosition(createFakePositionProps()),
        );
        await act(() => fireBotMoveMade());

        await assertDialog("move line");
    });

    it("should set game end dialog when resultData appears", async () => {
        getDialogForGameEndMock.mockReturnValue("end line");

        const botColor = GameColor.BLACK;
        renderComponent(botColor);

        await assertNoDialog("end line");

        const resultData = createFakeGameResultData();
        act(() => liveStore.setState({ resultData }));

        await assertDialog("end line");
        expect(getDialogForGameEndMock).toHaveBeenCalledExactlyOnceWith(
            resultData.result,
            botColor,
            liveStore.getState().gameToken,
        );
    });
});
