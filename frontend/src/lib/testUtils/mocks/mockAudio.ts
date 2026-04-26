import { Mock } from "vitest";

type AudioEvent = "ended";

interface AudioMock {
    play: Mock;
    pause: Mock;
    cloneNode: Mock;
    load: Mock;
    currentTime: number;
    preload: "none" | "metadata" | "auto" | "";
    addEventListener(event: string, callback: () => void): void;
}

interface MockAudioOptions {
    manuallyTriggerEnded?: boolean;
}

export function mockAudio({ manuallyTriggerEnded }: MockAudioOptions = {}): {
    audioMock: AudioMock;
    audioConstructorMock: Mock;
    listeners: Partial<Record<AudioEvent, () => void>>;
} {
    manuallyTriggerEnded ??= false;

    const listeners: Partial<Record<AudioEvent, () => void>> = {};
    const audioMock: AudioMock = {
        play: vi.fn(),
        pause: vi.fn(),
        cloneNode: vi.fn(() => audioMock),
        load: vi.fn(),
        currentTime: 0,
        preload: "",

        addEventListener: vi.fn((event: AudioEvent, callback: () => void) => {
            listeners[event] = callback;

            if (event === "ended" && !manuallyTriggerEnded) {
                callback();
            }
        }),
    };

    const audioConstructorMock = vi.fn().mockImplementation(function () {
        return audioMock;
    });
    vi.stubGlobal("Audio", audioConstructorMock);

    return { audioMock, audioConstructorMock, listeners };
}
