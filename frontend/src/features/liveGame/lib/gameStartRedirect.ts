import { useRouter } from "next/navigation";

import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import constants from "@/lib/constants";

export default async function gameStartRedirect(
    gameToken: string,
    router: ReturnType<typeof useRouter>,
): Promise<void> {
    await AudioPlayer.playAudio(AudioType.GAME_START);
    router.push(`${constants.PATHS.GAME}/${gameToken}`);
}
