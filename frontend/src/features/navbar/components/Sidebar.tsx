import { ArrowLeftIcon, ArrowRightIcon } from "@heroicons/react/24/outline";
import Image from "next/image";
import Link from "next/link";
import clsx from "clsx";

import getSidebarCollapseWidthCls from "../lib/sidebarWidth";
import { LowerNavItems, UpperNavItems } from "./NavItems";
import LogoText from "@public/assets/logo-text.svg";
import useCookieValue from "@/hooks/useCookieValue";
import Logo from "@public/assets/logo-no-bg.svg";
import constants from "@/lib/constants";
import NavItem from "./NavItem";

const Sidebar = ({
    isCollapsed,
    toggleCollapse,
}: {
    isCollapsed: boolean;
    toggleCollapse?: () => void;
}) => {
    const isLoggedIn = useCookieValue(constants.COOKIES.IS_LOGGED_IN, false);

    return (
        <aside
            className={clsx(
                `bg-navbar fixed z-50 flex h-full flex-col justify-between
                gap-10 overflow-auto border-r border-white/30 p-5 text-3xl`,
                isCollapsed && "items-center",
                getSidebarCollapseWidthCls(isCollapsed ?? false),
            )}
            data-testid="sidebar"
            data-is-collapsed={isCollapsed}
            aria-label="sidebar"
        >
            <ul
                className={clsx(
                    "flex flex-col gap-5",
                    isCollapsed && "items-center",
                )}
            >
                <Link
                    href="/"
                    prefetch={false}
                    className="hidden pb-3 md:flex"
                    data-testid="sidebarLogo"
                >
                    {isCollapsed ? (
                        <Image
                            src={Logo}
                            alt="Logo"
                            width={60}
                            height={60}
                            className="w-auto"
                        />
                    ) : (
                        <Image
                            src={LogoText}
                            alt="Logo with text"
                            height={60}
                            width={200}
                            className="w-auto self-center"
                            loading="eager"
                        />
                    )}
                </Link>

                <UpperNavItems
                    isLoggedIn={isLoggedIn}
                    isCollapsed={isCollapsed}
                />
            </ul>

            <ul className="flex flex-col gap-5 opacity-70">
                <LowerNavItems
                    isLoggedIn={isLoggedIn}
                    isCollapsed={isCollapsed}
                />

                {/* Collapse button */}
                <NavItem
                    as="button"
                    className="hidden lg:flex"
                    data-testid="sidebarCollapseButton"
                    icon={isCollapsed ? <ArrowRightIcon /> : <ArrowLeftIcon />}
                    onClick={toggleCollapse}
                    isCollapsed={isCollapsed}
                >
                    Collapse
                </NavItem>
            </ul>
        </aside>
    );
};
export default Sidebar;
