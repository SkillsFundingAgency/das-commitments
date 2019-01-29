$date_now = Get-Date
$extended_date = $date_now.AddYears(10)
$cert = New-SelfSignedCertificate -certstorelocation cert:\localmachine\my -dnsname localhost, localhost -notafter $extended_date -KeyLength 4096
$pwd = ConvertTo-SecureString -String ‘password’ -Force -AsPlainText
$path = ‘cert:\localMachine\my\’ + $cert.thumbprint
Export-PfxCertificate -cert $path -FilePath certificates\localhost.pfx -Password $pwd