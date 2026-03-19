using System;
using System.Collections.Generic;
using System.Text;

namespace StudioHub.Models;

public enum UserAvatar {
    Orso = 0,
    Ape = 1,
    Gatto = 2,
    Delfino = 3,
    Aquila = 4,
    Volpe = 5,
    Colibri = 6,
    Leone = 7,
    Polipo = 8,
    Gufo = 9,
    Procione = 10,
    Salmone = 11
}

public class AvatarItem {
    public UserAvatar Type { get; set; }
    public string? ImagePath { get; set; }
}
