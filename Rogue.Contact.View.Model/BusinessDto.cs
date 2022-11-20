﻿namespace Rogue.Contacts.View.Model;

public sealed record BusinessDto(string Name, string OwnerUsername, DateTime CreatedAt, IReadOnlyCollection<RoleDto> Roles);
