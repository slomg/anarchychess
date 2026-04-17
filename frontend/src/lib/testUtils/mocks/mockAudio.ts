import { Mock } from "vitest";

interface AudioMock {
    play: Mock;
    pause: Mock;
    cloneNode: Mock;
    load: Mock;
    currentTime: number;
    preload: "none" | "metadata" | "auto" | "";
}

export function mockAudio(): {
    audioMock: AudioMock;
    audioConstructorMock: Mock;
} {
    const audioMock: AudioMock = {
        play: vi.fn(),
        pause: vi.fn(),
        cloneNode: vi.fn(() => audioMock),
        load: vi.fn(),
        currentTime: 0,
        preload: "",
    };

    const audioConstructorMock = vi.fn().mockImplementation(function () {
        return audioMock;
    });
    vi.stubGlobal("Audio", audioConstructorMock);

    return { audioMock, audioConstructorMock };
}
