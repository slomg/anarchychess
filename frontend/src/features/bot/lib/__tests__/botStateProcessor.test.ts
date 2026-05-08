import { createFakeBotGameState } from "@/lib/testUtils/fakers/botGameStateFaker";
import processBotGameState from "../botStateProcessor";
import { LiveChessStoreProps } from "@/features/liveGame/stores/liveChessStore";
import { GameColor } from "@/lib/apiClient";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";

describe("processBotGameState", () => {
    it("should build live state", () => {
        const gameState = createFakeBotGameState();
        const viewerUserId = gameState.blackPlayer.userId;

        const { live } = processBotGameState(
            "game-token",
            viewerUserId,
            gameState,
        );

        expect(live).toEqual<LiveChessStoreProps>({
            gameToken: "game-token",
            sourceRevision: 0,

            whitePlayer: gameState.whitePlayer,
            blackPlayer: gameState.blackPlayer,
            sideToMove: gameState.sideToMove,

            pool: null,
            viewer: {
                userId: viewerUserId,
                playerColor: GameColor.BLACK,
            },

            drawState: null,
            liveClocks: null,
            clockSnapshotByPly: new Map(),
            resultData: null,
        });
    });

    it("should build board state", () => {
        const gameState = createFakeBotGameState({
            moveHistory: [
                createFakeMoveSnapshot(),
                createFakeMoveSnapshot(),
                createFakeMoveSnapshot(),
            ],
        });
        const viewerUserId = gameState.blackPlayer.userId;

        const { board } = processBotGameState(
            "game-token",
            viewerUserId,
            gameState,
        );

        expect(board.positionHistory?.totalPlyCount).toBe(3);
        expect(board.viewingFrom).toBe(GameColor.BLACK);
    });
});
