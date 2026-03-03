"use client";

import {
    PlayIcon,
    CpuChipIcon,
    Cog6ToothIcon,
    UserCircleIcon,
    BoltSlashIcon,
    CalendarIcon,
    MagnifyingGlassIcon,
    ArrowLeftEndOnRectangleIcon,
    BookOpenIcon,
} from "@heroicons/react/24/outline";

import { HeartIcon } from "@heroicons/react/16/solid";

import Link from "next/link";

import constants from "@/lib/constants";
import NavItem from "./NavItem";

export const UpperNavItems = ({
    isLoggedIn,
    isCollapsed,
}: {
    isLoggedIn?: boolean;
    isCollapsed: boolean;
}) => {
    return (
        <>
            <NavItem
                as={Link}
                href={constants.PATHS.PLAY}
                icon={<PlayIcon />}
                isCollapsed={isCollapsed}
            >
                Play
            </NavItem>

            <NavItem
                as={Link}
                href={constants.PATHS.BOT}
                icon={<CpuChipIcon />}
                isCollapsed={isCollapsed}
            >
                Computer
            </NavItem>

            <NavItem
                as={Link}
                href={constants.PATHS.ANALYSIS}
                icon={<MagnifyingGlassIcon />}
                isCollapsed={isCollapsed}
            >
                Analysis
            </NavItem>

            <NavItem
                as={Link}
                href={constants.PATHS.QUESTS}
                icon={<CalendarIcon />}
                isCollapsed={isCollapsed}
            >
                Quests
            </NavItem>

            {isLoggedIn && (
                <NavItem
                    as={Link}
                    href="/profile"
                    icon={<UserCircleIcon />}
                    isCollapsed={isCollapsed}
                >
                    Profile
                </NavItem>
            )}
            <NavItem
                as={Link}
                href={constants.PATHS.DONATE}
                icon={<HeartIcon color="red" />}
                isCollapsed={isCollapsed}
            >
                Donate
            </NavItem>

            {isLoggedIn === false && (
                <NavItem
                    as={Link}
                    href={constants.PATHS.SIGNIN}
                    icon={<ArrowLeftEndOnRectangleIcon />}
                    isCollapsed={isCollapsed}
                    className="text-secondary rounded-md"
                >
                    <div className="flex flex-col gap-1">
                        <span>Sign In</span>
                    </div>
                </NavItem>
            )}
        </>
    );
};

export const LowerNavItems = ({
    isLoggedIn,
    isCollapsed,
}: {
    isLoggedIn?: boolean;
    isCollapsed: boolean;
}) => {
    const authedLinks = (
        <>
            <NavItem
                as={Link}
                href={constants.PATHS.SETTINGS_BASE}
                icon={<Cog6ToothIcon />}
                isCollapsed={isCollapsed}
            >
                Settings
            </NavItem>

            <NavItem
                as={Link}
                href={constants.PATHS.LOGOUT}
                icon={<BoltSlashIcon />}
                isCollapsed={isCollapsed}
            >
                Logout
            </NavItem>
        </>
    );
    return (
        <>
            <NavItem
                as={Link}
                href={constants.PATHS.GUIDE}
                icon={<BookOpenIcon />}
                isCollapsed={isCollapsed}
            >
                Guide
            </NavItem>
            {isLoggedIn && authedLinks}
        </>
    );
};
