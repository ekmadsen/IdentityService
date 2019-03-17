select u.Username, c.[Type], uc.[Value]
from [Identity].UserClaims uc
inner join [Identity].Users u on uc.UserId = u.Id
inner join [Identity].Claims c on uc.ClaimId = c.Id
where u.Username = 'emadsen'
order by c.[Type] asc, uc.[Value] asc