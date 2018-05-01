pragma solidity ^0.4.21;

import "openzeppelin-solidity/contracts/math/SafeMath.sol";

contract Multiplier {

    event Multiplied(uint256 indexed multiplicand, uint256 indexed multiplier, address indexed sender, uint256 product);

    function multiply(uint256 a, uint256 b) public returns(uint r) {

        r = SafeMath.mul(a, b);

        emit Multiplied(a, b, msg.sender, r);

        return r;
    }
}