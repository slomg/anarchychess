import { Procedure } from "@vitest/spy";
import { useRouter } from "next/navigation";
import { Mock } from "vitest";

export interface RouterMock {
    back: Mock<Procedure>;
    forward: Mock<Procedure>;
    refresh: Mock<Procedure>;
    push: Mock<Procedure>;
    replace: Mock<Procedure>;
    prefetch: Mock<Procedure>;
}

export function mockRouter(): RouterMock {
    const router = {
        back: vi.fn(),
        forward: vi.fn(),
        refresh: vi.fn(),
        push: vi.fn(),
        replace: vi.fn(),
        prefetch: vi.fn(),
    };
    const routerMock = vi.mocked(useRouter);
    routerMock.mockImplementation(() => router);

    return router;
}
