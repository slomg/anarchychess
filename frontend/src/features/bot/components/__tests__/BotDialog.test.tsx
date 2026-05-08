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
import BotDialog, {
    BOT_DIALOG_PUNCTUATION_SPEED_MS,
    BOT_DIALOG_TYPING_SPEED_MS,
} from "../BotDialog";

import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakeGameResultData } from "@/lib/testUtils/fakers/gameResultDataFaker";
import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import PositionHistory from "@/features/chessboard/lib/positionHistory";
import useBotDialog, { DialogContext } from "../../hooks/useBotDialog";
import { BotClientEvents, useBotEvent } from "../../hooks/useBotHub";
import { decodeMovePath } from "@/features/liveGame/lib/moveDecoder";
import { BotType, GameColor, MoveSnapshot } from "@/lib/apiClient";
import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { PlayerType } from "@/features/liveGame/lib/types";

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

        vi.useFakeTimers();
    });

    function renderComponent(botColor?: GameColor) {
        return render(
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <LiveChessStoreContext.Provider value={liveStore}>
                    <BotDialog
                        botColor={botColor ?? GameColor.WHITE}
                        chessboardStore={chessboardStore}
                        botType={BotType.ANARCHY_BOT}
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

    function getVisibleDialogText(): string {
        const dialog = screen.queryByTestId("botDialog");
        if (!dialog) {
            return "";
        }

        return [...dialog.querySelectorAll("span")]
            .filter((x) => !x.classList.contains("invisible"))
            .map((x) => x.textContent)
            .join("");
    }

    async function assertDialog(expectedDialog: string) {
        await act(() =>
            vi.advanceTimersByTime(
                300 + (expectedDialog.length - 1) * BOT_DIALOG_TYPING_SPEED_MS,
            ),
        );

        expect(getVisibleDialogText()).toBe(expectedDialog);
    }

    async function assertNoDialog(fakeDialog?: string) {
        await act(() =>
            vi.advanceTimersByTime(
                300 +
                    (fakeDialog?.length ?? 0 - 1) * BOT_DIALOG_TYPING_SPEED_MS,
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

        const expectedMove = decodeMovePath(moveSnapshot.path);
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

        await act(() => vi.advanceTimersByTime(300));

        expect(getVisibleDialogText()).toBe("f");
    });

    it("should type out the dialog character by character", async () => {
        const dialogLine = "ab!c,de.f?g";
        getDialogForMoveMock.mockReturnValue(dialogLine);

        renderComponent();
        await fireBotMoveMade();
        await act(() => vi.advanceTimersByTime(300));

        expect(getVisibleDialogText()).toBe("a");

        await act(() => vi.advanceTimersByTime(BOT_DIALOG_TYPING_SPEED_MS));
        expect(getVisibleDialogText()).toBe("ab");

        await act(() => vi.advanceTimersByTime(BOT_DIALOG_TYPING_SPEED_MS));
        expect(getVisibleDialogText()).toBe("ab!");

        await act(() =>
            vi.advanceTimersByTime(BOT_DIALOG_PUNCTUATION_SPEED_MS),
        );
        expect(getVisibleDialogText()).toBe("ab!c");

        await act(() => vi.advanceTimersByTime(BOT_DIALOG_TYPING_SPEED_MS));
        expect(getVisibleDialogText()).toBe("ab!c,");

        await act(() =>
            vi.advanceTimersByTime(BOT_DIALOG_PUNCTUATION_SPEED_MS),
        );
        expect(getVisibleDialogText()).toBe("ab!c,d");

        await act(() => vi.advanceTimersByTime(BOT_DIALOG_TYPING_SPEED_MS));
        expect(getVisibleDialogText()).toBe("ab!c,de");

        await act(() => vi.advanceTimersByTime(BOT_DIALOG_TYPING_SPEED_MS));
        expect(getVisibleDialogText()).toBe("ab!c,de.");

        await act(() =>
            vi.advanceTimersByTime(BOT_DIALOG_PUNCTUATION_SPEED_MS),
        );
        expect(getVisibleDialogText()).toBe("ab!c,de.f");

        await act(() => vi.advanceTimersByTime(BOT_DIALOG_TYPING_SPEED_MS));
        expect(getVisibleDialogText()).toBe("ab!c,de.f?");

        await act(() =>
            vi.advanceTimersByTime(BOT_DIALOG_PUNCTUATION_SPEED_MS),
        );
        expect(getVisibleDialogText()).toBe("ab!c,de.f?g");
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
