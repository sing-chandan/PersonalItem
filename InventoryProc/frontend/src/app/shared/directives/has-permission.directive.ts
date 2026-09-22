import { Directive, Input, TemplateRef, ViewContainerRef, OnInit } from '@angular/core';
import { RoleService, Permission } from '../../core/services/role.service';

@Directive({
  selector: '[hasPermission]',
  standalone: true
})
export class HasPermissionDirective implements OnInit {
  private permission!: Permission;

  @Input() set hasPermission(permission: Permission) {
    this.permission = permission;
    this.updateView();
  }

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private roleService: RoleService
  ) {}

  ngOnInit(): void {
    this.updateView();
  }

  private updateView(): void {
    if (this.roleService.hasPermission(this.permission)) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
}
