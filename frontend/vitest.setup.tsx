import { setWindowInnerWidth } from "@/lib/testUtils/mocks/mockDom";
import { loadEnvFile } from "node:process";
import "vitest-dom/extend-expect";

import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import { mockAudio } from "@/lib/testUtils/mocks/mockAudio";

vi.mock("next/navigation");
vi.mock("@microsoft/signalr");

loadEnvFile();

window.HTMLMediaElement.prototype.load = () => {};

beforeEach(() => {
    window.ResizeObserver ??= vi.fn(
        class {
            disconnect = vi.fn();
            observe = vi.fn();
            unobserve = vi.fn();
        },
    );

    setWindowInnerWidth(1920);
    mockAudio();
    mockRouter();
});

afterEach(() => {
    vi.useRealTimers();
    vi.resetAllMocks();
    localStorage.clear();
});
