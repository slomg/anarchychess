export const UpperNavItems = vi.fn(
    ({ isLoggedIn }: { isLoggedIn: boolean }) => (
        <div data-testid="upperNavItems" data-is-logged-in={isLoggedIn}>
            UpperNavItems
        </div>
    ),
);
export const LowerNavItems = vi.fn(
    ({ isLoggedIn }: { isLoggedIn: boolean }) => (
        <div data-testid="lowerNavItems" data-is-logged-in={isLoggedIn}>
            LowerNavItems
        </div>
    ),
);
