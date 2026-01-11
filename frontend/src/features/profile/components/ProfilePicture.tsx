import Image from "next/image";

export interface ProfilePictureProps {
    userId: string;
    size?: number;
    className?: string;
    refreshKey?: number;
}

const ProfilePicture = ({
    userId,
    size = 120,
    className,
    refreshKey,
}: ProfilePictureProps) => {
    let url = `${process.env.NEXT_PUBLIC_API_URL}/api/Profile/profile-picture/${userId}`;
    if (refreshKey !== undefined) url += `?${refreshKey}`;

    return (
        <div className={className} style={{ width: size, height: size }}>
            <Image
                data-testid="profilePicture"
                className={"aspect-square rounded-md"}
                src={url}
                width={size}
                height={size}
                alt="profile picture"
                unoptimized
            />
        </div>
    );
};
export default ProfilePicture;
