import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import AudioPlayer, { AudioType } from "../audioPlayer";
import { mockAudio } from "@/lib/testUtils/mocks/mockAudio";

describe("AudioPlayer", () => {
    beforeEach(() => {
        // @ts-expect-error reset private static property for testing
        AudioPlayer._cachedAudios = new Map();
    });

    describe("playAudio", () => {
        it("should create a new Audio instance and play it if not cached", async () => {
            const { audioMock, audioConstructorMock } = mockAudio();
            await AudioPlayer.playAudio(AudioType.MOVE);

            expect(audioConstructorMock).toHaveBeenCalledWith(
                `${process.env.NEXT_PUBLIC_ASSETS_URL}/sfx/${AudioType.MOVE}`,
            );
            expect(audioMock.cloneNode).toHaveBeenCalledOnce();
            expect(audioMock.play).toHaveBeenCalledOnce();
        });

        it("should reuse cached Audio instance on subsequent calls", async () => {
            const { audioConstructorMock } = mockAudio();

            await AudioPlayer.playAudio(AudioType.MOVE);
            await AudioPlayer.playAudio(AudioType.CAPTURE);
            await AudioPlayer.playAudio(AudioType.MOVE);
            await AudioPlayer.playAudio(AudioType.CAPTURE);

            expect(audioConstructorMock).toHaveBeenCalledTimes(2);
            expect(audioConstructorMock).toHaveBeenCalledWith(
                `${process.env.NEXT_PUBLIC_ASSETS_URL}/sfx/${AudioType.MOVE}`,
            );
            expect(audioConstructorMock).toHaveBeenCalledWith(
                `${process.env.NEXT_PUBLIC_ASSETS_URL}/sfx/${AudioType.CAPTURE}`,
            );
        });

        it("should reset currentTime to 0 before playing", async () => {
            const { audioMock } = mockAudio();
            audioMock.currentTime = 5;

            await AudioPlayer.playAudio(AudioType.CAPTURE);

            expect(audioMock.currentTime).toBe(0);
            expect(audioMock.play).toHaveBeenCalled();
        });

        it("should wait for ended event before resolving", async () => {
            const { audioMock, listeners } = mockAudio({
                manuallyTriggerEnded: true,
            });

            let resolved = false;
            const promise = AudioPlayer.playAudio(AudioType.MOVE).then(() => {
                resolved = true;
            });

            expect(resolved).toBe(false);
            expect(audioMock.play).toHaveBeenCalledOnce();

            await flushMicrotasks();
            listeners["ended"]?.();

            await promise;

            expect(resolved).toBe(true);
        });
    });

    describe("preload", () => {
        it("should create and cache audio instance for each type", () => {
            const { audioConstructorMock, audioMock } = mockAudio();

            AudioPlayer.preload(AudioType.MOVE, AudioType.CAPTURE);

            expect(audioConstructorMock).toHaveBeenCalledTimes(2);
            expect(audioConstructorMock).toHaveBeenCalledWith(
                `${process.env.NEXT_PUBLIC_ASSETS_URL}/sfx/${AudioType.MOVE}`,
            );
            expect(audioConstructorMock).toHaveBeenCalledWith(
                `${process.env.NEXT_PUBLIC_ASSETS_URL}/sfx/${AudioType.CAPTURE}`,
            );

            audioConstructorMock.mockReset();

            AudioPlayer.playAudio(AudioType.MOVE);
            expect(audioConstructorMock).not.toHaveBeenCalled();
            expect(audioMock.cloneNode).toHaveBeenCalled();
            expect(audioMock.play).toHaveBeenCalled();
        });

        it("should set preload to auto and call load", () => {
            const { audioMock } = mockAudio();

            AudioPlayer.preload(AudioType.MOVE);

            expect(audioMock.preload).toBe("auto");
            expect(audioMock.load).toHaveBeenCalledOnce();
        });

        it("should not recreate audio if already cached", () => {
            const { audioConstructorMock } = mockAudio();

            AudioPlayer.preload(AudioType.MOVE);
            AudioPlayer.preload(AudioType.MOVE);

            expect(audioConstructorMock).toHaveBeenCalledTimes(1);
        });
    });
});
