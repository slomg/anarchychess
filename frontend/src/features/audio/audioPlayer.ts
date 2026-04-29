export enum AudioType {
    MOVE = "move.webm",
    CAPTURE = "capture.webm",
    ILLEGAL_MOVE = "illegal.webm",
    PROMOTION = "promotion.webm",

    EXPLOSION = "explosion.webm",
    CASTLE = "castle.webm",

    LOW_TIME = "low_time.webm",

    GAME_START = "game_start.webm",
    GAME_END = "game_end.webm",
}

const AUDIO_PATH = `${process.env.NEXT_PUBLIC_ASSETS_URL}/sfx/`;

export default class AudioPlayer {
    private static _cachedAudios: Map<AudioType, HTMLAudioElement> = new Map();

    static async playAudio(audioType: AudioType): Promise<void> {
        let audio = this._cachedAudios.get(audioType);
        if (!audio) {
            audio = new Audio(AUDIO_PATH + audioType);
            this._cachedAudios.set(audioType, audio);
        }

        const clone = audio.cloneNode() as HTMLAudioElement;
        clone.currentTime = 0;
        await clone.play();

        await new Promise<void>((resolve) => {
            clone.addEventListener("ended", () => resolve(), { once: true });
        });
    }

    static preload(...audioTypes: AudioType[]): void {
        for (const type of audioTypes) {
            if (this._cachedAudios.has(type)) {
                continue;
            }

            const audio = new Audio(AUDIO_PATH + type);
            audio.preload = "auto";
            audio.load();

            this._cachedAudios.set(type, audio);
        }
    }
}
