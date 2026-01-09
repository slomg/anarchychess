import SessionProvider from "@/features/auth/contexts/sessionContext";
import { Renderable, renderRenderable } from "@/lib/utils/renderable";
import { fetchAuthedUserSession } from "../lib/getLoggedIn";
import RefreshRedirect from "../components/RefreshRedirect";
import { type PrivateUser } from "@/lib/apiClient";

interface WithAuthedUserProps {
    user: PrivateUser;
    accessToken: string;
}

export default async function WithAuthedUser({
    children,
}: {
    children: Renderable<WithAuthedUserProps>;
}) {
    const session = await fetchAuthedUserSession();
    if (!session) return <RefreshRedirect />;

    return (
        <SessionProvider user={session.user} fetchAttempted>
            {renderRenderable(children, session)}
        </SessionProvider>
    );
}
