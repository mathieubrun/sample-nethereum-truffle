var Multiplier = artifacts.require("./Multiplier.sol");

contract('Multiplier', function () {
  let contract = null;

  describe("with deployment", function() {
    beforeEach('deploy contract', async function() {
      contract = await Multiplier.deployed();
    })
  
    it("should multiply", async function () {
      let res = await contract.multiply.call(4, 5);
      
      assert.equal(res, 20);
    });
  });

  describe("with new", function() {
    beforeEach('create contract', async function() {
      contract = await Multiplier.new();
    })
  
    it("should multiply", async function () {
      let res = await contract.multiply.call(4, 5);
      
      assert.equal(res, 20);
    });
  });
});