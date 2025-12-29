import { useEffect, useEffectEvent, useRef } from "react";

import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import { GameState } from "@/lib/apiClient";

export default function useGameStartAudio(gameState: GameState) {
    const playedRef = useRef(false);

    const playStartAudio = useEffectEvent(() => {
        if (playedRef.current) return;

        if (gameState.moveHistory.length === 0 && !gameState.resultData) {
            AudioPlayer.playAudio(AudioType.GAME_START);
            playedRef.current = true;
        }
    });
    useEffect(() => playStartAudio(), []);
}
