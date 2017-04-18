import React, { Component } from 'react'
import Panel from './Panel'

class ToolbarPanel extends Component {
  render() {
    return (
      <Panel title={this.props.title}>
        <div className="btn-toolbar">
          {this.props.actions.map((action, i) => {
            return <div key={i} className="btn-group">
              <button className={'btn ' + action.className} onClick={action.onClick}>{action.title}</button>
            </div>;
          })}
        </div>
      </Panel>
    );
  }
}

export default ToolbarPanel;
