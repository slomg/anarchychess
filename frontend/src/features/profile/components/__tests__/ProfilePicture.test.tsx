import { render, screen } from "@testing-library/react";

import Image from "next/image";

import ProfilePicture from "@/features/profile/components/ProfilePicture";

vi.mock("next/image");

describe("ProfilePicture", () => {
    it("should render with default props", () => {
        const { queryByAltText } = render(<ProfilePicture userId="test" />);
        const profilePicture = queryByAltText("profile picture");

        expect(profilePicture).toBeInTheDocument();
    });

    it("should render with custom props", () => {
        const className = "test-class";
        const userId = "testuser";
        const size = 150;
        const minSize = 100;

        render(
            <ProfilePicture
                userId={userId}
                size={size}
                minSize={minSize}
                className={className}
            />,
        );

        expect(Image).toHaveBeenCalledWith(
            expect.objectContaining({
                src: `${process.env.NEXT_PUBLIC_API_URL}/api/Profile/profile-picture/${userId}`,
                alt: "profile picture",
                width: size,
                height: size,
                unoptimized: true,
            }),
            undefined,
        );
        const pfp = screen.getByAltText("profile picture");
        const parent = pfp.parentElement;
        expect(parent?.className).toBe(className);
        expect(parent).toHaveStyle({
            width: size + "px",
            height: size + "px",
            minWidth: minSize + "px",
            minHeight: minSize + "px",
        });
    });

    it("should add refreshKey if provided", () => {
        const userId = "testuser";
        const refreshKey = 123;

        render(<ProfilePicture userId={userId} refreshKey={refreshKey} />);

        expect(Image).toHaveBeenCalledWith(
            expect.objectContaining({
                src: `${process.env.NEXT_PUBLIC_API_URL}/api/Profile/profile-picture/${userId}?${refreshKey}`,
            }),
            undefined,
        );
    });
});
