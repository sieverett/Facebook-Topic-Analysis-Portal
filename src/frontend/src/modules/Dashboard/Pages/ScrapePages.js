import React, { Component } from 'react';
import { getPageScrapes, exportPages, scrapePages } from '../Common/Data/Actions';
import Panel from '../Components/Common/Panel';
import PageSelectionList from '../Components/Common/PageSelectionList';
import ExportPages from '../Components/ScrapePages/ExportPages';
import PageScrapeList from '../Components/ScrapePages/PageScrapeList';

class ScrapePages extends Component {
  // Load the up-to-date scrape history each time the page is refreshed or loaded.
  componentWillMount = () => this.getScrapes();

  getScrapes = (newPageNumber, newPageSize, since, until) => {
    const { pageNumber, pageSize } = this.context.store.getState().pageScrapes;
    this.context.store.dispatch(getPageScrapes(newPageNumber || pageNumber, newPageSize || pageSize, since, until));
  }

  handleExport = (contentType, since, until) => exportPages(contentType, since, until, (_, errorMessage) => {});
  
  handlePageScrapeClicked = (data, index) => window.location.href += '/' + data.id;

  handleScrapePages = (pages) => this.context.store.dispatch(scrapePages(pages));

  render() {
    const { pages, pageScrapes, errorMessage } = this.context.store.getState();

    return (
      <section>
        <PageSelectionList title="Scrape" onSubmit={this.handleScrapePages} />
        <section className="col-md-8">
          <ExportPages onSubmit={(since, until) => this.getScrapes(null, null, since, until)} onExport={contentType => this.handleExport(contentType, pages.since, pages.until)} />
          <Panel showHeading={false} table={true}>
            <PageScrapeList scrapes={pageScrapes} errorMessage={errorMessage} onRowSelected={this.handlePageScrapeClicked} />
          </Panel>
        </section>
      </section>
    );
  }
}
ScrapePages.contextTypes = {store: React.PropTypes.object};

export default ScrapePages;
