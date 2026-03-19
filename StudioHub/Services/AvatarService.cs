using System;
using System.Collections.Generic;
using System.Text;

namespace StudioHub.Services;

public class AvatarService {

    private const string BASE_PATH = "pack://application:,,,/StudioHub;component/Assets/UserAvatars/";

    public static List<AvatarItem> GetAvailableAvatars() {

        List<AvatarItem> avatars = [];
        foreach (UserAvatar avatarType in Enum.GetValues<UserAvatar>()) {
            AvatarItem item = new() {
                Type = avatarType,
                ImagePath = BASE_PATH + avatarType.ToString() + ".png"
            };
            avatars.Add(item);
        }
        return avatars;
    }

    public static string GetPathByType(UserAvatar type) {
        return BASE_PATH + type.ToString() + ".png";
    }
}
