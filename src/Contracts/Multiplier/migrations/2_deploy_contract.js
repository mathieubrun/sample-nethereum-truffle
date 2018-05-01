var Multiplier = artifacts.require("./Multiplier.sol");

module.exports = function(deployer) {
  deployer.deploy(Multiplier);
};
