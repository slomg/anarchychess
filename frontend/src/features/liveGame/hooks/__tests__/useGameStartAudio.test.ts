import { renderHook } from "@testing-library/react";
import useGameStartAudio from "../useGameStartAudio";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { createFakeGameState } from "@/lib/testUtils/fakers/gameStateFaker";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import { GameResult } from "@/lib/apiClient";

vi.mock("@/features/audio/audioPlayer");

describe("useGameStartAudio", () => {
    const freshGameState = createFakeGameState({
        moveHistory: [],
        resultData: null,
    });

    const startedGameState = createFakeGameState({
        moveHistory: [createFakeMoveSnapshot()],
        resultData: null,
    });

    it("should play the game start audio on page load when the game is fresh", () => {
        renderHook(() => useGameStartAudio(freshGameState));

        expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
            AudioType.GAME_START,
        );
    });

    it("should not play the game start audio on page load when the game is not fresh", () => {
        renderHook(() => useGameStartAudio(startedGameState));

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();
    });

    it("should not replay the game start audio when game state changes after mount", () => {
        const { rerender } = renderHook(
            ({ gameState }) => useGameStartAudio(gameState),
            {
                initialProps: { gameState: freshGameState },
            },
        );

        rerender({ gameState: startedGameState });

        expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
            AudioType.GAME_START,
        );
    });

    it("should only play the game start audio once even if re-rendered with the same fresh state", () => {
        const { rerender } = renderHook(
            ({ gameState }) => useGameStartAudio(gameState),
            {
                initialProps: { gameState: freshGameState },
            },
        );

        rerender({ gameState: freshGameState });

        expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
            AudioType.GAME_START,
        );
    });

    it("should not play the game start audio on page load when the game was aborted", () => {
        const abortedGameState = createFakeGameState({
            moveHistory: [],
            resultData: {
                result: GameResult.ABORTED,
                resultDescription: "aborted",
            },
        });

        renderHook(() => useGameStartAudio(abortedGameState));

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();
    });
});
