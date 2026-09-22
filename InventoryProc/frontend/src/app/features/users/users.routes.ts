import { Routes } from '@angular/router';
import { UserListComponent } from './components/user-list/user-list.component';
import { UserFormComponent } from './components/user-form/user-form.component';
import { UserProfileComponent } from './components/user-profile/user-profile.component';
import { UserActivityComponent } from './components/user-activity/user-activity.component';
import { ActivityLogsComponent } from './components/activity-logs/activity-logs.component';

export const USERS_ROUTES: Routes = [
  {
    path: '',
    component: UserListComponent
  },
  {
    path: 'create',
    component: UserFormComponent
  },
  {
    path: 'edit/:id',
    component: UserFormComponent
  },
  {
    path: 'profile',
    component: UserProfileComponent
  },
  {
    path: 'activity-logs',
    component: ActivityLogsComponent
  },
  {
    path: ':id/activity',
    component: UserActivityComponent
  }
];
