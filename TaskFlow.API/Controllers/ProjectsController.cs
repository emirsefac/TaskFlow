using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Controllers;

[Authorize]
[Route("api/projects")]
public class ProjectsController : BaseApiController
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>Giriş yapan kullanıcının üyesi olduğu tüm projeleri listeler.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetMyProjects()
        => Ok(await _projectService.GetMyProjectsAsync(CurrentUserId));

    /// <summary>Bir projenin detayını (üyeler + görevler) getirir.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDetailDto>> GetById(int id)
        => Ok(await _projectService.GetByIdAsync(id, CurrentUserId));

    /// <summary>Yeni proje oluşturur; oluşturan kişi otomatik Owner olur.</summary>
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectDto dto)
    {
        var project = await _projectService.CreateAsync(dto, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    /// <summary>Projeye email ile yeni üye ekler (sadece Owner yapabilir).</summary>
    [HttpPost("{id:int}/members")]
    public async Task<IActionResult> AddMember(int id, AddMemberDto dto)
    {
        await _projectService.AddMemberAsync(id, dto, CurrentUserId);
        return NoContent();
    }
}
