FROM ethereum/client-go:v1.8.6

COPY ./data /data

RUN geth init data/genesis_clique.json

RUN geth account import data/account1.key --password data/account1.pass
RUN geth account import data/account2.key --password data/account2.pass

ENTRYPOINT [ "geth", "--nodiscover", "-unlock=4efd4ab8293b3b8179d9dbf9ade1ae00d83eb314", "--password=data/account1.pass", "--maxpeers=0", "--rpc", "--rpcapi=eth,web3,personal,net,miner,admin,debug", "--rpcvhosts=geth", "--rpcaddr=0.0.0.0", "--mine" ]           