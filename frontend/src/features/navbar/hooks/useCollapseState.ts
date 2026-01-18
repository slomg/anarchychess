import { useEffect, useState } from "react";

import useLocalPref from "@/hooks/useLocalPref";
import constants from "@/lib/constants";

function useCollapseState(): { isCollapsed: boolean; toggleCollapse(): void } {
    const [isCollapsedPref, setIsCollapsedPref] = useLocalPref(
        constants.LOCALSTORAGE.IS_SIDEBAR_COLLAPSED,
        false,
    );
    const [isCollapsed, setIsCollapsed] = useState<boolean>(isCollapsedPref);

    useEffect(() => {
        const handleResize = () => {
            const isSmallScreen = window.innerWidth < 1024;
            if (isSmallScreen) {
                setIsCollapsed(true);
            } else {
                setIsCollapsed(isCollapsedPref);
            }
        };

        handleResize();
        window.addEventListener("resize", handleResize);
        return () => window.removeEventListener("resize", handleResize);
    }, [isCollapsedPref]);

    function toggleCollapse() {
        setIsCollapsedPref((prev) => {
            const newState = !prev;
            setIsCollapsed(newState);
            return newState;
        });
    }

    return { isCollapsed, toggleCollapse };
}
export default useCollapseState;
