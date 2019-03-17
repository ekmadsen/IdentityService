select u.Username, r.Name
from [Identity].UserRoles ur
inner join [Identity].Users u on ur.UserId = u.Id
inner join [Identity].Roles r on ur.RoleId = r.Id
where u.Username = 'emadsen'
order by r.Name asc