using System;
using System.Collections.Generic;
using System.Text;

namespace StudioHub.Models;

public record WordTemplate(
    Guid Id,
    string Name,
    string Description,
    byte[] FileContent,
    string SessionGuid
);
