# Grant developer permission to host website at URL.
netsh http add urlacl url=https://dev.brainstorm.com:443/ user="Erik"
netsh http add urlacl url=https://dev.brainstorm.com:44300/ user="Erik"
netsh http add urlacl url=https://dev.madpoker.net:443/ user="Erik"
netsh http add urlacl url=https://dev.madpoker.net:44300/ user="Erik"
netsh http add urlacl url=https://dev.madpoker.net:44301/ user="Erik"


# Remove developer permission to host website at URL.
netsh http delete urlacl url=https://dev.brainstorm.com:443/
netsh http delete urlacl url=https://dev.brainstorm.com:44300/
netsh http delete urlacl url=https://dev.madpoker.net:443/
netsh http delete urlacl url=https://dev.madpoker.net:44300/
netsh http delete urlacl url=https://dev.madpoker.net:44301/


# List the certificates in Local Computer store.
Get-ChildItem -path "Cert:\LocalMachine\My"


#List the certificates bound to HTTP ports.
netsh http show sslcert


# Bind certificate to HTTP port (for use by IIS Express website).
# May need to run netsh first, then enter http command, then enter add sslcert... command.
netsh http add sslcert hostnameport=dev.brainstorm.com:443 certhash=a481c2d0f016ab62e0e2c447ea63b2fea5e718b7 appid={214124cd-d05b-4309-9af9-9caa44b2b74a} certstore=my
netsh http add sslcert hostnameport=dev.brainstorm.com:44300 certhash=a481c2d0f016ab62e0e2c447ea63b2fea5e718b7 appid={214124cd-d05b-4309-9af9-9caa44b2b74a} certstore=my
netsh http add sslcert hostnameport=dev.madpoker.net:443 certhash=ba96768e69269a2d38e9c43b823fd21d5611f84d appid={214124cd-d05b-4309-9af9-9caa44b2b74a} certstore=my
netsh http add sslcert hostnameport=dev.madpoker.net:44300 certhash=ba96768e69269a2d38e9c43b823fd21d5611f84d appid={214124cd-d05b-4309-9af9-9caa44b2b74a} certstore=my
netsh http add sslcert hostnameport=dev.madpoker.net:44301 certhash=ba96768e69269a2d38e9c43b823fd21d5611f84d appid={214124cd-d05b-4309-9af9-9caa44b2b74a} certstore=my


# Unbind certificate from HTTP port (for use by IIS Express website).
netsh http delete sslcert hostnameport=dev.brainstorm.com:443
netsh http delete sslcert hostnameport=dev.brainstorm.com:44300
netsh http delete ssscert hostnameport=dev.madpoker.net:443
netsh http delete ssscert hostnameport=dev.madpoker.net:44300
netsh http delete ssscert hostnameport=dev.madpoker.net:44301