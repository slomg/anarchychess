"use client";

import useCollapseState from "../hooks/useCollapseState";
import getSidebarCollapseWidthCls from "../lib/sidebarWidth";
import Sidebar from "./Sidebar";
import clsx from "clsx";

const NavDesktop = () => {
    const { isCollapsed, toggleCollapse } = useCollapseState();

    return (
        <section
            className={clsx(
                getSidebarCollapseWidthCls(isCollapsed),
                "hidden shrink-0 transition-[width] md:block",
            )}
            data-testid="navDesktop"
        >
            <Sidebar
                isCollapsed={isCollapsed}
                toggleCollapse={toggleCollapse}
            />
        </section>
    );
};

export default NavDesktop;
