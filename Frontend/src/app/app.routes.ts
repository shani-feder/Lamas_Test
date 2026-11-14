import { Routes } from '@angular/router';
import { ControlListComponent } from './components/control-list/control-list.component';
import { ControlEditorComponent } from './components/control-editor/control-editor.component';
import { ControlResultsComponent } from './components/control-results/control-results.component';
import { AiSuggestionsComponent } from './components/ai-suggestions/ai-suggestions.component';

export const routes: Routes = [
  { path: '', redirectTo: '/controls', pathMatch: 'full' },
  { path: 'controls', component: ControlListComponent },
  { path: 'controls/new', component: ControlEditorComponent },
  { path: 'controls/:id', component: ControlEditorComponent },
  { path: 'controls/:id/results', component: ControlResultsComponent },
  { path: 'ai-suggestions', component: AiSuggestionsComponent }
];
