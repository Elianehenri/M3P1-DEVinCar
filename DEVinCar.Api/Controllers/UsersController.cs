﻿using DEVinCar.Api.Models;
using DEVinCar.Api.Data;
using DEVinCar.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DEVinCar.Api.Controllers;

[ApiController]
[Route("api/user")]

public class UserController : ControllerBase
{
    private readonly DevInCarDbContext _context;

    public UserController(DevInCarDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<List<User>> Get(
       [FromQuery] string Name,
       [FromQuery] DateTime? birthDateMax,
       [FromQuery] DateTime? birthDateMin
   )
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(Name))
        {
            query = query.Where(c => c.Name.Contains(Name));
        }

        if (birthDateMin.HasValue)
        {
            query = query.Where(c => c.BirthDate >= birthDateMin.Value);
        }

        if (birthDateMax.HasValue)
        {
            query = query.Where(c => c.BirthDate <= birthDateMax.Value);
        }

        if (!query.ToList().Any())
        {
            return NoContent();
        }

        return Ok(
            query
            .ToList()
            );
    }

    [HttpGet("{id}")]
    public ActionResult<User> GetById(
        [FromRoute] int id
    )
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpDelete("{userId}")]
    public ActionResult Delete(
        [FromRoute] int userId
    )
    {
        var user = _context.Users.Find(userId);

        if (user == null)
        {
            return NotFound();
        }
        _context.Users.Remove(user);
        _context.SaveChanges();

        return NoContent();
    }

    [HttpPost]
    public ActionResult<User> Post(
        [FromBody] UserDTO userDto
    )
    {
        var newUser = _context.Users.FirstOrDefault(u => u.Email == userDto.Email);

        if (newUser != null)
        {
            return BadRequest();
        }

        newUser = new User
        {
            Name = userDto.Name,
            Email = userDto.Email,
            Password = userDto.Password,
            BirthDate = userDto.BirthDate
        };

        _context.Users.Add(newUser);
        _context.SaveChanges();

        return Created("api/users", newUser.Id);
    }
   
    [HttpGet("{userId}/buy")]
    public ActionResult<Sale> GetByIdbuy(
        [FromRoute] int userId)


    {
        var sales = _context.Sales.Where(s => s.BuyerId == userId);

        if (sales == null || sales.Count() == 0)
        {
            return NoContent();
        }
        return Ok(sales.ToList());
    }


    [HttpPost("{userId}/sales")]
    public ActionResult<Sale> PostSaleUserId(
           [FromBody] SaleDTO body)
    {

        if (_context.Sales.Any(s => s.BuyerId == null || body.BuyerId == 0))
        {
            return BadRequest();
        }

        if (_context.Sales.Any(s => s.BuyerId != body.UserId || s.SellerId != body.UserId))
        {
            return NotFound();
        }

        var sale = new Sale
        {
            BuyerId = body.BuyerId,
            SellerId = body.UserId,
            SaleDate = body.SaleDate,
        };
        _context.Sales.Add(sale);
        _context.SaveChanges();
        return Created("api/sale", sale.Id);


    }
}





