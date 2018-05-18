require('chai')
  .use(require('chai-as-promised'))
  .use(require('chai-bignumber')(web3.BigNumber))
  .should();

var Multiplier = artifacts.require("./Multiplier.sol");

contract('Multiplier', function () {
  let contract = null;

  beforeEach('deploy', async function() {
    contract = await Multiplier.deployed();
  })

  describe("multiply", function() {
    it("should multiply simple numbers", async function () {
      (await contract.multiply.call(2, 2)).should.bignumber.equal(4);
    });

    it("should multiply BigNumbers", async function () {
      const bg = new web3.BigNumber(2, 10);
      const multiplicand = bg.pow(64);
      const multiplier = bg.pow(128);
      const product = bg.pow(192);
      
      var result = await contract.multiply.call(multiplicand, multiplier)
      
      result.should.bignumber.equal(product);
    });

    it("should not overflow", async function () {
      // uint256 max value
      const max = new web3.BigNumber(2, 10).pow(256).sub(1);

      await contract.multiply.call(max, 2).should.eventually.be.rejected;
    });
  });
});